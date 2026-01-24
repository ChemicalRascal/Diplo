using BSBackupSystem.Data;
using BSBackupSystem.Model.App;
using BSBackupSystem.Model.Diplo;
using BSBackupSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Security.Claims;
using System.Security.Policy;

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

        builder.Services.AddEmailManager(builder.Configuration);
        builder.Services.AddScoped<IUserStore<User>, AppUserStore>();
        builder.Services.AddScoped<IRoleStore<AppRole>, AppRoleStore>();
        builder.Services.AddDbContext<AppDbContext>(doDatabaseSetup);
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddIdentity<User, AppRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
        }).AddDefaultTokenProviders();

        // TODO: Bundle this up somewhere descriptive
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnSignedIn += async (context) =>
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                if (context.Principal?.Identity?.Name is null)
                {
                    // TODO: Verify that this leads to auth failure
                    return;
                }
                var dbUser = await userManager.FindByNameAsync(context.Principal.Identity.Name);
                if (dbUser is null)
                {
                    // TODO: As above. Also how? Logging would be good here
                    return;
                }

                if (dbUser.EmailConfirmed)
                {
                    // TODO: define claims in one location
                    var emailConfirmed = new Claim("EmailConfirmed", "Confirmed");
                    context.Principal.AddIdentity(new ClaimsIdentity([emailConfirmed]));
                }
                else
                {
                    context.Options.AccessDeniedPath = "/Identity/Account/ResendEmailConfirmation";
                }
            };
        });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("EmailMustBeConfirmed", policyBuilder =>
            {
                policyBuilder.RequireClaim("EmailConfirmed", "Confirmed");
            });

        //TODO: Requisite encryption classes
        //.AddPersonalDataProtection()
        builder.Services.AddRazorPages();
        builder.Services.AddQuartz(q =>
        {
            q.RegisterCoreJobs();
        });
        builder.Services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

        builder.Services.AddDiploDataManager();
        builder.Services.AddGameReader();

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
           .WithStaticAssets()
           //.RequireAuthorization("EmailMustBeConfirmed")
           ;

        await app.AddUserRolesAsync();
        await app.AddSeededUsersAsync();

        app.Run();

        Console.WriteLine($"BSBackupSystem has terminated. Goodbye!");
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
