using BSBackupSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BSBackupSystem.Model.App;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BSBackupSystem;

public class Program
{
    private const string POSTGRES = "Postgres";

    public static void Main(string[] args)
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

        app.Run();
    }
}
