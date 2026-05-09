using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nimble.Modulith.Reporting.Data;
using Nimble.Modulith.Reporting.Services;
using Serilog;

namespace Nimble.Modulith.Reporting;

public static class ReportingModuleExtensions
{
    public static IHostApplicationBuilder AddReportingModuleServices(
        this IHostApplicationBuilder builder,
        Serilog.ILogger logger)
    {
        builder.AddSqlServerDbContext<ReportingDbContext>("reportingdb");
        builder.Services.AddScoped<IReportService, ReportService>();

        logger.Information("{Module} module services registered",
            nameof(ReportingModuleExtensions).Replace("ModuleExtensions", ""));

        return builder;
    }

    public static async Task<WebApplication> EnsureReportingModuleDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReportingDbContext>>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        
        try
        {
            logger.LogInformation("Ensuring reporting database exists...");
            
            // In development, drop and recreate to ensure seed data is applied
            if (env.IsDevelopment())
            {
                logger.LogInformation("Development mode: Dropping and recreating reporting database to ensure seed data...");
                await context.Database.EnsureDeletedAsync();
                var created = await context.Database.EnsureCreatedAsync();
                if (created)
                {
                    logger.LogInformation("Reporting database recreated successfully with seed dates");
                }
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Reporting database ensured");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure reporting database exists");
            throw;
        }
        
        return app;
    }
}
