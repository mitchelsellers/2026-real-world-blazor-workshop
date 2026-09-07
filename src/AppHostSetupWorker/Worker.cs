using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using System.Diagnostics;

namespace AppHostSetupWorker;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity(
            "Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(
        ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(
            state: dbContext,
            operation: static async (context, _, ct) =>
            {
                // Run migration in a transaction to avoid partial migration if it fails.
                await context.Database.MigrateAsync(ct);
                return 0;
            },
            verifySucceeded: null,
            cancellationToken: cancellationToken);
    }

    private static async Task SeedDataAsync(
        ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        //TODO: If needed
    }
}