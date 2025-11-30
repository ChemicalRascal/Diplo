using System.ComponentModel;
using System.Security.Claims;
using BSBackupSystem.Model.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Data;

public class AppRoleStore : AppRoleStore<AppRole>
{
    public AppRoleStore(AppDbContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

public class AppRoleStore<TRole> : AppRoleStore<TRole, DbContext, Guid>
    where TRole : AppRole<Guid>
{
    public AppRoleStore(DbContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

public class AppRoleStore<TRole, TContext> : AppRoleStore<TRole, TContext, Guid>
    where TRole : AppRole<Guid>
    where TContext : DbContext
{
    public AppRoleStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

public class AppRoleStore<TRole, TContext, TKey> : AppRoleStore<TRole, TContext, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>,
    IQueryableRoleStore<TRole>,
    IRoleClaimStore<TRole>
    where TRole : AppRole<TKey>
    where TKey : IEquatable<TKey>
    where TContext : DbContext
{
    public AppRoleStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

public class AppRoleStore<TRole, TContext, TKey, TUserRole, TRoleClaim> :
    IQueryableRoleStore<TRole>,
    IRoleClaimStore<TRole>
    where TRole : AppRole<TKey>
    where TKey : IEquatable<TKey>
    where TContext : DbContext
    where TUserRole : IdentityUserRole<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    public AppRoleStore(TContext context, IdentityErrorDescriber? describer = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
        ErrorDescriber = describer ?? new IdentityErrorDescriber();
    }

    private bool _disposed;

    public virtual TContext Context { get; private set; }

    public IdentityErrorDescriber ErrorDescriber { get; set; }

    public bool AutoSaveChanges { get; set; } = true;

    protected virtual async Task SaveChanges(CancellationToken cancellationToken)
    {
        if (AutoSaveChanges)
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public virtual async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        Context.Add(role);
        await SaveChanges(cancellationToken);
        return IdentityResult.Success;
    }

    public virtual async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        Context.Attach(role);
        role.ConcurrencyStamp = Guid.NewGuid().ToString();
        Context.Update(role);
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

    public virtual async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        Context.Remove(role);
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

    public virtual Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        return Task.FromResult(ConvertIdToString(role.Id)!);
    }

    public virtual Task<string?> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        return Task.FromResult(role.Name);
    }

    public virtual Task SetRoleNameAsync(TRole role, string? roleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public virtual TKey? ConvertIdFromString(string id)
    {
        if (id == null)
        {
            return default(TKey);
        }
        return (TKey?)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
    }

    public virtual string? ConvertIdToString(TKey id)
    {
        if (id.Equals(default(TKey)))
        {
            return null;
        }
        return id.ToString();
    }

    public virtual Task<TRole?> FindByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var roleId = ConvertIdFromString(id);
        return Roles.FirstOrDefaultAsync(u => u.Id.Equals(roleId), cancellationToken);
    }

    public virtual Task<TRole?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        return Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
    }

    public virtual Task<string?> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        return Task.FromResult(role.NormalizedName);
    }

    public virtual Task SetNormalizedRoleNameAsync(TRole role, string? normalizedName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose() => _disposed = true;

    public virtual async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);

        return await RoleClaims.Where(rc => rc.RoleId.Equals(role.Id)).Select(c => new Claim(c.ClaimType!, c.ClaimValue!)).ToListAsync(cancellationToken);
    }

    public virtual Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(claim);

        RoleClaims.Add(CreateRoleClaim(role, claim));
        return Task.FromResult(false);
    }

    public virtual async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(claim);
        var claims = await RoleClaims.Where(rc => rc.RoleId.Equals(role.Id) && rc.ClaimValue == claim.Value && rc.ClaimType == claim.Type).ToListAsync(cancellationToken);
        foreach (var c in claims)
        {
            RoleClaims.Remove(c);
        }
    }

    public virtual IQueryable<TRole> Roles => Context.Set<TRole>();

    private DbSet<TRoleClaim> RoleClaims { get { return Context.Set<TRoleClaim>(); } }

    protected virtual TRoleClaim CreateRoleClaim(TRole role, Claim claim)
        => new TRoleClaim { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value };
}
