# Setup Aspire

The goal of this step is to take our current Aspire relationship and move it to the next level, and have Aspire manage a Database for Us and automatically apply DB Migrations for us.

>[!NOTE]
>You must have docker running, and the latest SQL image for this to work.  if you don't have this setup, you can simply skip over this step.

## Create Setup Worker Project

This is going to be our process that will ensure our Migrations are auto-applied when using Aspire to run things

* Right click on the solution and select `Add New Project`
* Select `Worker Service (C#)` as the project type
* Name it `ProjectHub.AppHostSetupWorker` and select `Next`
* Leave the defaults as-is on the next screen and select `Create`
* Using the "Package Management Console" pointed to the AppHostSetupWorker project run the following command to install a needed package `Install-Package Aspire.Microsoft.EntityFrameworkCore.SqlServer`
* Add a reference to the `ProjectHub.Data` project as well.  (Right click "Add" -> "Project Reference")

### Update `Worker.cs`

Replace the current code with the below

```` csharp
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using System.Diagnostics;

namespace ProjectHub.AppHostSetupWorker;

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
````

### Update `Program.cs`

Repace the current code with the below

```` csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProjectHub.AppHostSetupWorker;
using ProjectHub.Data;
using ProjectHub.Data.Models;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

//Be sure to use the proper connection string mapping here
builder.AddSqlServerDbContext<ApplicationDbContext>("DefaultConnection");

//Add identity required items so that our model's are correct (Long-Term COnsider a shared/setup Method)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var host = builder.Build();
host.Run();

````

## Add Support for Conditional usage of Aspire Managed

Sometimes you may want to use a remote DB, sometimes you may want it to be stand alone, for this purpose we have an easy to manage way to do this.  Add a setting that we can inspect at start.

### Update Configuration
Open the `appsettings.Development.json` within the `ProjectHub.AppHost` project and add the following setting above the existing "Logging" section

```` json
"UseRemoteDatabase": false,
````

### Add NuGet Reference

To enable SQL Server support for Aspire, in the `AppHost` project run the following command to add a reference.

```` powershell
Install-Package Aspire.Hosting.SqlServer
````

### Update Aspire Startup Sequence

If we need to, we want to start with a common setup, replace `AppHost.cs` with the below

```` csharp
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var useRemoteDb = builder.Configuration.GetValue<bool>("UseRemoteDatabase");

//Do we want to do additional overides, or do se use a locally configured DB
if (!useRemoteDb)
{
    // Latest SQL, Persistent to ensure faster startup, and a named DB
    // NOTE: Persistent DB here DOES ensure that a DB is re-used across runs, but it does not ensure that the DB is cleaned up between runs. If you want a clean DB each time, use ContainerLifetime.Transient instead of Persistent.
    var sql = builder.AddSqlServer("sql")
        .WithImageTag("2025-latest")
        .WithLifetime(ContainerLifetime.Persistent)
        .AddDatabase("DefaultConnection", "ProjectHubDb20261");

    // Run the migrations, with a reference to SQL so the connection information is injected, and wait for SQL to be ready
    var migrations = builder.AddProject<Projects.ProjectHub_AppHostSetupWorker>("Migrations")
        .WithReference(sql)
        .WaitFor(sql);

    // Lastly add the Web project, with references to both SQL and Migrations, and wait for both to be ready before starting the Web project
    builder.AddProject<Projects.ProjectHub_Web>("projecthub-web")
        .WithReference(sql)
        .WithReference(migrations)
        .WaitFor(sql)
        .WaitFor(migrations)
        .WithExternalHttpEndpoints();
}
else
{
    builder.AddProject<Projects.ProjectHub_Web>("web")
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
````
