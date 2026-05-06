using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nimble.Modulith.Email.Integrations;
using Nimble.Modulith.Email.Interfaces;
using Nimble.Modulith.Email.Services;
using Serilog;

namespace Nimble.Modulith.Email;

public static class EmailModuleExtensions
{
    public static WebApplicationBuilder AddEmailModuleServices(
        this WebApplicationBuilder builder,
        ILogger logger)
    {
        logger.Information("Adding Email module services...");

        // Configure email settings from appsettings.json
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

        // Singleton: thread-safe, shared connection pool
        builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

        // Singleton queue — shared across all scopes/requests
        builder.Services.AddSingleton(typeof(IQueueService<>), typeof(ChannelQueueService<>));

        // Background worker
        builder.Services.AddHostedService<EmailSendingBackgroundWorker>();

        logger.Information("Email module services added successfully");

        return builder;
    }
}
