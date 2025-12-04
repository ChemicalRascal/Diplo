using BSBackupSystem.Model.Diplo;
using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Data;

public class AppDbContext : AppIdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    public DbSet<Game> Games { get; set; }
    public DbSet<MoveSet> MoveSets { get; set; }
    public DbSet<UnitOrder> UnitOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Game>(b =>
        {
            b.HasMany(g => g.MoveSets).WithOne();
        });

        builder.Entity<MoveSet>(b =>
        {
            b.HasMany(ms => ms.Orders).WithOne();
        });

        builder.Entity<HoldOrder>();

        builder.Entity<MoveOrder>(b =>
        {
            b.Property(o => o.To)
             .HasColumnName("to");
            b.Property(o => o.ToCoast)
             .HasColumnName("to_coast");
            b.Ignore(o => o.ViaConvoy);
        });

        builder.Entity<SupportHoldOrder>()
            .Property(o => o.Supporting)
            .HasColumnName("to");

        builder.Entity<SupportMoveOrder>(b =>
        {
            b.Property(o => o.SupportingFrom)
             .HasColumnName("from");
            b.Property(o => o.SupportingTo)
             .HasColumnName("to");
        });

        builder.Entity<ConvoyOrder>(b =>
        {
            b.Property(o => o.ConvoyFrom)
             .HasColumnName("from");
            b.Property(o => o.ConvoyTo)
             .HasColumnName("to");
        });

        builder.Entity<RetreatOrder>(b =>
        {
            b.Property(o => o.To)
             .HasColumnName("to");
            b.Property(o => o.ToCoast)
             .HasColumnName("to_coast");
        });

        builder.Entity<BuildOrder>();

        builder.Entity<DisbandOrder>();
    }
}
