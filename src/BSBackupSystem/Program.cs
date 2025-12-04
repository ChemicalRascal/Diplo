using BSBackupSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BSBackupSystem.Model.App;
using Microsoft.IdentityModel.Tokens;
using BSBackupSystem.Model.Diplo;
using BSBackupSystem.Services;

namespace BSBackupSystem;

public class Program
{
    private const string POSTGRES = "Postgres";

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Action<DbContextOptionsBuilder> doDatabaseSetup = options =>
        {
            var connectionString = builder.Configuration.GetConnectionString(POSTGRES)
                ?? throw new InvalidOperationException($"Connection string '{POSTGRES}' not found.");
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        };

        builder.Services.AddScoped<IUserStore<User>, AppUserStore>();
        builder.Services.AddScoped<IRoleStore<AppRole>, AppRoleStore>();
        builder.Services.AddDbContext<AppDbContext>(doDatabaseSetup);
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddIdentity<User, AppRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
        }).AddDefaultTokenProviders();
            //TODO: Requisite encryption classes
            //.AddPersonalDataProtection()
        builder.Services.AddRazorPages();

        builder.Services.AddScoped<DiploDataManager>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // TODO: Read: https://aka.ms/aspnetcore-hsts
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.MapControllerRoute(
            name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        //await app.AddMockGame();
        await app.AddUserRolesAsync();
        await app.AddSeededUsersAsync();

        app.Run();
    }
}

public static class SetupExtensions
{
    private static void ThrowOnFailure(IdentityResult? result)
    {
        if ((!result?.Succeeded) ?? true)
        {
            throw new ApplicationException($"Couldn't seed data: {result}");
        }
    }

    private class SeedUser
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
    }

    extension(WebApplication app)
    {
        public async Task AddMockGame()
        {
            using (var scope = app.Services.CreateScope())
            {
                var appDb = scope.ServiceProvider.GetService<AppDbContext>();

                var game = new Game() { Uri = "", ForeignId = "", CreationTime = DateTime.UnixEpoch };
                game.MoveSets.Add(new() { State = "", FullHash = "", PreRetreatHash = "", SeasonIndex = 0, Year = 0 });
                game.MoveSets[0].Orders.AddRange([
                    new HoldOrder() { Player = "", Result = "", ResultReason = "", Unit = "", UnitCoast = null, UnitType = "" },
                    new MoveOrder() { Player = "", Result = "", ResultReason = "", Unit = "", UnitCoast = null, UnitType = "", To = "" },
                    new SupportHoldOrder() { Player = "", Result = "", ResultReason = "", Unit = "", UnitCoast = null, UnitType = "", Supporting = "" },
                    new SupportMoveOrder() { Player = "", Result = "", ResultReason = "", Unit = "", UnitCoast = null, UnitType = "", SupportingFrom = "", SupportingTo = "" },
                    new ConvoyOrder() { Player = "", Result = "", ResultReason = "", Unit = "", UnitCoast = null, UnitType = "", ConvoyFrom = "", ConvoyTo = "" },
                    new RetreatOrder() { Player = "", Result = "", ResultReason = "", Unit = "", UnitCoast = null, UnitType = "", To = "" },
                    new BuildOrder() { Player = "", Result = "", ResultReason = "", Unit = "", UnitCoast = null, UnitType = "" },
                    new DisbandOrder() { Player = "", Result = "", ResultReason = "", Unit = "", UnitCoast = null, UnitType = "" },
                    ]);
                appDb!.Add(game);
                await appDb.SaveChangesAsync();
            }
        }

        public async Task AddUserRolesAsync()
        {
            var supportedRoles = Enum.GetValues<UserRole>().Except([UserRole.Unknown]).Select(Enum.GetName);
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetService<RoleManager<AppRole>>();
                foreach (var role in supportedRoles)
                {
                    if (!await roleManager!.RoleExistsAsync(role!))
                    {
                        ThrowOnFailure(await roleManager.CreateAsync(new(role!)));
                    }
                }
            }
        }

        public async Task AddSeededUsersAsync()
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetService<RoleManager<AppRole>>();
                var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
                var users = app.Configuration.GetSection("SeededUsers").Get<SeedUser[]>() ?? [];

                foreach (var user in users)
                {
                    if (user.Email.IsNullOrEmpty() || user.Password.IsNullOrEmpty())
                    {
                        continue;
                    }

                    var dbUser = await userManager!.FindByEmailAsync(user.Email);
                    if (dbUser is not null)
                    {
                        continue;
                    }

                    ThrowOnFailure(await userManager.CreateAsync(new User(user.Email)
                    {
                        Email = user.Email,
                        EmailConfirmed = true,
                    }, user.Password));
                    dbUser = await userManager!.FindByEmailAsync(user.Email);
                    ThrowOnFailure(await userManager.AddToRolesAsync(dbUser!, user.Roles));
                }
            }
        }
    }
}
