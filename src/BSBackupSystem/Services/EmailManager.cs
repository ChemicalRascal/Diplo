using BSBackupSystem.Model.App;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.RateLimiting;

namespace BSBackupSystem.Services;

public class EmailManager(IFluentEmail emailer) : IEmailSender<User>, IEmailSender
{
    public const string CONFIG_SECTION = "EmailManager";

    private RateLimiter limiter = new SlidingWindowRateLimiter(new()
    {
        Window = TimeSpan.FromSeconds(30),
        PermitLimit = 10,
        QueueLimit = 30,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        AutoReplenishment = true,
        SegmentsPerWindow = 6,
    });

    async Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await emailer.To(email).Subject(subject)
            .Body(htmlMessage, isHtml: true)
            .SendAsync();
    }

    async Task IEmailSender<User>.SendConfirmationLinkAsync(User user, string email, string confirmationLink) => throw new NotImplementedException();
    async Task IEmailSender<User>.SendPasswordResetCodeAsync(User user, string email, string resetCode) => throw new NotImplementedException();
    async Task IEmailSender<User>.SendPasswordResetLinkAsync(User user, string email, string resetLink) => throw new NotImplementedException();

    public record Opts(string Domain, string ApiKey, string SendingAddress, string? SendingName);
}

public static partial class ServiceExtensions
{
    extension (IServiceCollection services)
    {
        public void AddEmailManager(IConfiguration configuration)
        {
            services.AddTransient<IEmailSender<User>, EmailManager>();
            services.AddTransient<IEmailSender, EmailManager>();

            var opts = configuration.GetRequiredSection(EmailManager.CONFIG_SECTION)
                .Get<EmailManager.Opts>()
                ?? throw new ApplicationException("Failed to bind Email Manager configuration.");

            services.AddFluentEmail(opts.SendingAddress, opts.SendingName)
                .AddMailGunSender(opts.Domain, opts.ApiKey);
        }
    }
}