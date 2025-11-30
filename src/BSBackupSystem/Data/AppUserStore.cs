using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BSBackupSystem.Model.App;

namespace BSBackupSystem.Data;

/* Pretty much all of this is taken from AspNetCore.Identity.
 * In retrospect, deciding to force using our own User class
 * was, well, signing up for a lot more work than expected.
 */

public class AppUserStore : AppUserStore<User>
{
    public AppUserStore(AppDbContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

public class AppUserStore<TUser> : AppUserStore<TUser, AppRole, DbContext, Guid>
    where TUser : User<Guid>, new()
{
    public AppUserStore(DbContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

public class AppUserStore<TUser, TRole, TContext> : AppUserStore<TUser, TRole, TContext, Guid>
    where TUser : User<Guid>
    where TRole : AppRole<Guid>
    where TContext : DbContext
{
    public AppUserStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

public class AppUserStore<TUser, TRole, TContext, TKey>
    : AppUserStore<TUser, TRole, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
    where TUser : User<TKey>
    where TRole : AppRole<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
{
    public AppUserStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

public class AppUserStore<TUser, TRole, TContext, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
    : AppUserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>,
    IProtectedUserStore<TUser>
    where TUser : User<TKey>
    where TRole : AppRole<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    public AppUserStore(TContext context, IdentityErrorDescriber? describer = null) : base(describer ?? new IdentityErrorDescriber())
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }

    public virtual TContext Context { get; private set; }

    private DbSet<TUser> UsersSet { get { return Context.Set<TUser>(); } }
    private DbSet<TRole> Roles { get { return Context.Set<TRole>(); } }
    private DbSet<TUserClaim> UserClaims { get { return Context.Set<TUserClaim>(); } }
    private DbSet<TUserRole> UserRoles { get { return Context.Set<TUserRole>(); } }
    private DbSet<TUserLogin> UserLogins { get { return Context.Set<TUserLogin>(); } }
    private DbSet<TUserToken> UserTokens { get { return Context.Set<TUserToken>(); } }

    public bool AutoSaveChanges { get; set; } = true;

    protected Task SaveChanges(CancellationToken cancellationToken)
    {
        return AutoSaveChanges ? Context.SaveChangesAsync(cancellationToken) : Task.CompletedTask;
    }

    public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        Context.Add(user);
        await SaveChanges(cancellationToken);
        return IdentityResult.Success;
    }

    public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        Context.Attach(user);
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        Context.Update(user);
        try
        {
            await SaveChanges(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }

    public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        Context.Remove(user);
        try
        {
            await SaveChanges(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }

    public override Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var id = ConvertIdFromString(userId);
        return UsersSet.FindAsync(new object?[] { id }, cancellationToken).AsTask();
    }

    public override Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    public override IQueryable<TUser> Users
    {
        get { return UsersSet; }
    }

    protected override Task<TRole?> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return Roles.SingleOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
    }

    protected override Task<TUserRole?> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
    {
        return UserRoles.FindAsync(new object[] { userId, roleId }, cancellationToken).AsTask();
    }

    protected override Task<TUser?> FindUserAsync(TKey userId, CancellationToken cancellationToken)
    {
        return Users.SingleOrDefaultAsync(u => u.Id.Equals(userId), cancellationToken);
    }

    protected override Task<TUserLogin?> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        return UserLogins.SingleOrDefaultAsync(userLogin => userLogin.UserId.Equals(userId) && userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey, cancellationToken);
    }

    protected override Task<TUserLogin?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        return UserLogins.SingleOrDefaultAsync(userLogin => userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey, cancellationToken);
    }

    protected override TUserRole CreateUserRole(TUser user, TRole role)
    {
        return new TUserRole()
        {
            UserId = user.Id,
            RoleId = role.Id
        };
    }

    public override async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedRoleName);

        var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (roleEntity == null)
        {
            throw new InvalidOperationException($"No role for '{normalizedRoleName}' found.");
        }
        UserRoles.Add(CreateUserRole(user, roleEntity));
    }

    public override async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedRoleName);

        var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (roleEntity != null)
        {
            var userRole = await FindUserRoleAsync(user.Id, roleEntity.Id, cancellationToken);
            if (userRole != null)
            {
                UserRoles.Remove(userRole);
            }
        }
    }

    public override async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;
        var query = from userRole in UserRoles
                    join role in Roles on userRole.RoleId equals role.Id
                    where userRole.UserId.Equals(userId)
                    select role.Name;
        return await query.ToListAsync(cancellationToken);
    }

    public override async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedRoleName);

        var role = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (role != null)
        {
            var userRole = await FindUserRoleAsync(user.Id, role.Id, cancellationToken);
            return userRole != null;
        }
        return false;
    }

    public override async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        return await UserClaims.Where(uc => uc.UserId.Equals(user.Id)).Select(c => c.ToClaim()).ToListAsync(cancellationToken);
    }

    public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claims);
        foreach (var claim in claims)
        {
            UserClaims.Add(CreateUserClaim(user, claim));
        }
        return Task.FromResult(false);
    }

    public override async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(newClaim);

        var matchedClaims = await UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
        foreach (var matchedClaim in matchedClaims)
        {
            matchedClaim.ClaimValue = newClaim.Value;
            matchedClaim.ClaimType = newClaim.Type;
        }
    }

    public override async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claims);
        foreach (var claim in claims)
        {
            var matchedClaims = await UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
            foreach (var c in matchedClaims)
            {
                UserClaims.Remove(c);
            }
        }
    }

    public override Task AddLoginAsync(TUser user, UserLoginInfo login,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(login);
        UserLogins.Add(CreateUserLogin(user, login));
        return Task.FromResult(false);
    }

    public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var entry = await FindUserLoginAsync(user.Id, loginProvider, providerKey, cancellationToken);
        if (entry != null)
        {
            UserLogins.Remove(entry);
        }
    }

    public override async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;
        return await UserLogins.Where(l => l.UserId.Equals(userId))
            .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToListAsync(cancellationToken);
    }

    public override async Task<TUser?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var userLogin = await FindUserLoginAsync(loginProvider, providerKey, cancellationToken);
        if (userLogin != null)
        {
            return await FindUserAsync(userLogin.UserId, cancellationToken);
        }
        return null;
    }

    public override Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Users.SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(claim);

        var query = from userclaims in UserClaims
                    join user in Users on userclaims.UserId equals user.Id
                    where userclaims.ClaimValue == claim.Value
                    && userclaims.ClaimType == claim.Type
                    select user;

        return await query.ToListAsync(cancellationToken);
    }

    public override async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(normalizedRoleName);

        var role = await FindRoleAsync(normalizedRoleName, cancellationToken);

        if (role != null)
        {
            var query = from userrole in UserRoles
                        join user in Users on userrole.UserId equals user.Id
                        where userrole.RoleId.Equals(role.Id)
                        select user;

            return await query.ToListAsync(cancellationToken);
        }
        return new List<TUser>();
    }

    protected override Task<TUserToken?> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        => UserTokens.FindAsync(new object[] { user.Id, loginProvider, name }, cancellationToken).AsTask();

    protected override Task AddUserTokenAsync(TUserToken token)
    {
        UserTokens.Add(token);
        return Task.CompletedTask;
    }

    protected override Task RemoveUserTokenAsync(TUserToken token)
    {
        UserTokens.Remove(token);
        return Task.CompletedTask;
    }
}

public abstract class AppUserStoreBase<TUser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey, TUserClaim, TUserLogin, TUserToken> :
    IUserLoginStore<TUser>,
    IUserClaimStore<TUser>,
    IUserPasswordStore<TUser>,
    IUserSecurityStampStore<TUser>,
    IUserEmailStore<TUser>,
    IUserLockoutStore<TUser>,
    IQueryableUserStore<TUser>,
    IUserTwoFactorStore<TUser>,
    IUserAuthenticationTokenStore<TUser>,
    IUserAuthenticatorKeyStore<TUser>,
    IUserTwoFactorRecoveryCodeStore<TUser>
    where TUser : User<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
{
    public AppUserStoreBase(IdentityErrorDescriber describer)
    {
        ArgumentNullException.ThrowIfNull(describer);

        ErrorDescriber = describer;
    }

    private bool _disposed;

    public IdentityErrorDescriber ErrorDescriber { get; set; }

    protected virtual TUserClaim CreateUserClaim(TUser user, Claim claim)
    {
        var userClaim = new TUserClaim { UserId = user.Id };
        userClaim.InitializeFromClaim(claim);
        return userClaim;
    }

    protected virtual TUserLogin CreateUserLogin(TUser user, UserLoginInfo login)
    {
        return new TUserLogin
        {
            UserId = user.Id,
            ProviderKey = login.ProviderKey,
            LoginProvider = login.LoginProvider,
            ProviderDisplayName = login.ProviderDisplayName
        };
    }

    protected virtual TUserToken CreateUserToken(TUser user, string loginProvider, string name, string? value)
    {
        return new TUserToken
        {
            UserId = user.Id,
            LoginProvider = loginProvider,
            Name = name,
            Value = value
        };
    }

    public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(ConvertIdToString(user.Id)!);
    }

    public virtual Task<string?> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.UserName);
    }

    public virtual Task SetUserNameAsync(TUser user, string? userName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.NormalizedUserName);
    }

    public virtual Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public abstract Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken));

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "TKey is annoated with RequiresUnreferencedCodeAttribute.All.")]
    public virtual TKey? ConvertIdFromString(string? id)
    {
        if (id == null)
        {
            return default;
        }
        return (TKey?)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
    }

    public virtual string? ConvertIdToString(TKey id)
    {
        if (object.Equals(id, default(TKey)))
        {
            return null;
        }
        return id.ToString();
    }

    public abstract Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken));

    public abstract IQueryable<TUser> Users
    {
        get;
    }

    public virtual Task SetPasswordHashAsync(TUser user, string? passwordHash, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.PasswordHash);
    }

    public virtual Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(user.PasswordHash != null);
    }

    protected abstract Task<TUser?> FindUserAsync(TKey userId, CancellationToken cancellationToken);

    protected abstract Task<TUserLogin?> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken);

    protected abstract Task<TUserLogin?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken);

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        _disposed = true;
    }

    public abstract Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));

    public virtual async Task<TUser?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var userLogin = await FindUserLoginAsync(loginProvider, providerKey, cancellationToken).ConfigureAwait(false);
        if (userLogin != null)
        {
            return await FindUserAsync(userLogin.UserId, cancellationToken).ConfigureAwait(false);
        }
        return null;
    }

    public virtual Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.EmailConfirmed);
    }

    public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public virtual Task SetEmailAsync(TUser user, string? email, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.Email = email;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.Email);
    }

    public virtual Task<string?> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.NormalizedEmail);
    }

    public virtual Task SetNormalizedEmailAsync(TUser user, string? normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public abstract Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken));

    public virtual Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.LockoutEnd);
    }

    public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    public virtual Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    public virtual Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    public virtual Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.AccessFailedCount);
    }

    public virtual Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.LockoutEnabled);
    }

    public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    public virtual Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(stamp);
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.SecurityStamp);
    }

    public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.TwoFactorEnabled = enabled;
        return Task.CompletedTask;
    }

    public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        return Task.FromResult(user.TwoFactorEnabled);
    }

    public abstract Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken));

    protected abstract Task<TUserToken?> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken);

    protected abstract Task AddUserTokenAsync(TUserToken token);

    protected abstract Task RemoveUserTokenAsync(TUserToken token);

    public virtual async Task SetTokenAsync(TUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(user);

        var token = await FindTokenAsync(user, loginProvider, name, cancellationToken).ConfigureAwait(false);
        if (token == null)
        {
            await AddUserTokenAsync(CreateUserToken(user, loginProvider, name, value)).ConfigureAwait(false);
        }
        else
        {
            token.Value = value;
        }
    }

    public virtual async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(user);
        var entry = await FindTokenAsync(user, loginProvider, name, cancellationToken).ConfigureAwait(false);
        if (entry != null)
        {
            await RemoveUserTokenAsync(entry).ConfigureAwait(false);
        }
    }

    public virtual async Task<string?> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(user);
        var entry = await FindTokenAsync(user, loginProvider, name, cancellationToken).ConfigureAwait(false);
        return entry?.Value;
    }

    private const string InternalLoginProvider = "[AspNetUserStore]";
    private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
    private const string RecoveryCodeTokenName = "RecoveryCodes";

    public virtual Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
        => SetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);

    public virtual Task<string?> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
        => GetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, cancellationToken);

    public virtual async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(user);
        var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken).ConfigureAwait(false) ?? "";
        if (mergedCodes.Length > 0)
        {
            return mergedCodes.AsSpan().Count(';') + 1;
        }
        return 0;
    }

    public virtual Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        var mergedCodes = string.Join(";", recoveryCodes);
        return SetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
    }

    public virtual async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNullOrEmpty(code);

        var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken).ConfigureAwait(false) ?? "";
        var splitCodes = mergedCodes.Split(';');
        if (splitCodes.Contains(code))
        {
            var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
            await ReplaceCodesAsync(user, updatedCodes, cancellationToken).ConfigureAwait(false);
            return true;
        }
        return false;
    }
}

public abstract class AppUserStoreBase<TUser, TRole, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
    : AppUserStoreBase<TUser, TKey, TUserClaim, TUserLogin, TUserToken>, IUserRoleStore<TUser>
    where TUser : User<TKey>
    where TRole : AppRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    public AppUserStoreBase(IdentityErrorDescriber describer) : base(describer) { }

    protected virtual TUserRole CreateUserRole(TUser user, TRole role)
    {
        return new TUserRole()
        {
            UserId = user.Id,
            RoleId = role.Id
        };
    }

    public abstract Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken));

    public abstract Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken));

    protected abstract Task<TRole?> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken);

    protected abstract Task<TUserRole?> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken);
}