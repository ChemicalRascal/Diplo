using BSBackupSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BSBackupSystem.Model.App;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.IdentityModel.Tokens;

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
            options.UseNpgsql(connectionString).UseLowerCaseNamingConvention();
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
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
           .WithStaticAssets();

        await app.AddUserRolesAsync();
        await app.AddSeededUsersAsync();

        app.Run();
    }
}

public static class SetupExtensions
{
    private static void throwOnFailure(IdentityResult? result)
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
        public string[] Roles { get; set; } = Array.Empty<string>();
    }

    extension(WebApplication app)
    {
        public async Task AddUserRolesAsync()
        {
            var desiredRoles = Enum.GetValues<UserRole>().Except([UserRole.Unknown]).Select(Enum.GetName);
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetService<RoleManager<AppRole>>();
                foreach (var role in desiredRoles)
                {
                    if (!await roleManager!.RoleExistsAsync(role!))
                    {
                        var createResult = await roleManager.CreateAsync(new(role!));
                        throwOnFailure(createResult);
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

                    throwOnFailure(await userManager.CreateAsync(new User(user.Email)
                    {
                        Email = user.Email,
                        EmailConfirmed = true,
                    }, user.Password));
                    dbUser = await userManager!.FindByEmailAsync(user.Email);
                    throwOnFailure(await userManager.AddToRolesAsync(dbUser!, user.Roles));
                }
            }
        }
    }
}
