using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BSBackupSystem.Model.App;

namespace BSBackupSystem.Data;

/* Pretty much all of this is taken from AspNetCore.Identity,
 * but it's maintained as its own thing here in order to be
 * much more able to control it.
 */

public abstract class AppIdentityDbContext
    : AppIdentityDbContext<User, AppRole, Guid>
{
    public AppIdentityDbContext(DbContextOptions options) : base(options) { }

    protected AppIdentityDbContext() { }
}

public abstract class AppIdentityDbContext<TUser, TRole, TKey>
    : AppIdentityDbContext<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
    where TUser : User<TKey>
    where TRole : AppRole<TKey>
    where TKey : IEquatable<TKey>
{
    public AppIdentityDbContext(DbContextOptions options) : base(options) { }

    protected AppIdentityDbContext() { }
}

public abstract class AppIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    : DbContext
    where TUser : User<TKey>
    where TRole : AppRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
    public AppIdentityDbContext(DbContextOptions options) : base(options) { }

    protected AppIdentityDbContext() { }

    public virtual DbSet<TUser> Users { get; set; } = default!;
    public virtual DbSet<TUserClaim> UserClaims { get; set; } = default!;
    public virtual DbSet<TUserLogin> UserLogins { get; set; } = default!;
    public virtual DbSet<TUserToken> UserTokens { get; set; } = default!;
    public virtual DbSet<TUserRole> UserRoles { get; set; } = default!;
    public virtual DbSet<TRole> Roles { get; set; } = default!;
    public virtual DbSet<TRoleClaim> RoleClaims { get; set; } = default!;

    private sealed class PersonalDataConverter : ValueConverter<string, string>
    {
        public PersonalDataConverter(IPersonalDataProtector protector) : base(s => protector.Protect(s), s => protector.Unprotect(s), default)
        { }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var maxKeyLength = 128;
        var protector = this.GetInfrastructure().GetService<IPersonalDataProtector>();
        PersonalDataConverter? converter = protector is not null
            ? new PersonalDataConverter(protector)
            : null;

        builder.Entity<TUser>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
            b.ToTable("SystemUsers");
            b.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();

            b.Property(u => u.UserName).HasMaxLength(256);
            b.Property(u => u.NormalizedUserName).HasMaxLength(256);
            b.Property(u => u.Email).HasMaxLength(256);
            b.Property(u => u.NormalizedEmail).HasMaxLength(256);

            if (converter is not null)
            {
                var personalDataProps = typeof(TUser).GetProperties().Where(
                                prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    if (p.PropertyType != typeof(string))
                    {
                        throw new InvalidOperationException("Personal Data Protection only works on strings.");
                    }
                    b.Property(typeof(string), p.Name).HasConversion(converter);
                }
            }

            b.HasMany<TUserClaim>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
            b.HasMany<TUserLogin>().WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
            b.HasMany<TUserToken>().WithOne().HasForeignKey(ut => ut.UserId).IsRequired();
            b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
        });

        builder.Entity<TUserClaim>(b =>
        {
            b.HasKey(uc => uc.Id);
            b.ToTable("SystemUserClaims");
        });

        builder.Entity<TUserLogin>(b =>
        {
            b.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            b.Property(l => l.LoginProvider).HasMaxLength(maxKeyLength);
            b.Property(l => l.ProviderKey).HasMaxLength(maxKeyLength);

            b.ToTable("SystemUserLogins");
        });

        builder.Entity<TUserToken>(b =>
        {
            b.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            b.Property(t => t.LoginProvider).HasMaxLength(maxKeyLength);
            b.Property(t => t.Name).HasMaxLength(maxKeyLength);

            if (converter is not null)
            {
                var tokenProps = typeof(TUserToken).GetProperties().Where(
                                prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
                foreach (var p in tokenProps)
                {
                    if (p.PropertyType != typeof(string))
                    {
                        throw new InvalidOperationException("Personal Data Protection only works on strings.");
                    }
                    b.Property(typeof(string), p.Name).HasConversion(converter);
                }
            }

            b.ToTable("SystemUserTokens");
        });

        builder.Entity<TRole>(b =>
        {
            b.HasKey(r => r.Id);
            b.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();
            b.ToTable("SystemRoles");
            b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

            b.Property(u => u.Name).HasMaxLength(256);
            b.Property(u => u.NormalizedName).HasMaxLength(256);

            b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();
            b.HasMany<TRoleClaim>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
        });

        builder.Entity<TRoleClaim>(b =>
        {
            b.HasKey(rc => rc.Id);
            b.ToTable("SystemRoleClaims");
        });

        builder.Entity<TUserRole>(b =>
        {
            b.HasKey(r => new { r.UserId, r.RoleId });
            b.ToTable("SystemUserRoles");
        });
    }
}
