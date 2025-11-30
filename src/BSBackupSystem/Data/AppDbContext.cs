using Microsoft.EntityFrameworkCore;

namespace BSBackupSystem.Data;

public class AppDbContext : AppIdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
