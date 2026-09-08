# Redis - Better Caching Support

## Add Redis to AppHost

Install the following package to the AppHost

```` powershell
Install-Package Aspire.Hosting.Redis
````

Add Redis to the initial setup in AppHost.cs

```` csharp
var redis = builder.AddRedis("redis");
````

Update the Web Project Setup to have a reference & waitfor redis

With Other stuff

```` csharp
builder.AddProject<Projects.ProjectHub_Web>("projecthub-web")
    .WithReference(sql)
    .WithReference(migrations)
    .WithReference(redis)
    .WaitFor(sql)
    .WaitFor(migrations)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();
````

Local Runner

```` charp
builder.AddProject<Projects.ProjectHub_Web>("web")
    .WithReference(redis)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();
````

>[!NOTE]
>Here I've done this with BOTH, as we don't have a remote Redis we can use, showing some of the flexibility.

## Setup Project for Redis now

Add package to the web project

```` powershell
Install-Package Microsoft.Extensions.Caching.StackExchangeRedis
````

Keeping our existing configuration, we want to add the configuration of redis, NOTE: We use the same name as referenced in Aspire!

Place after the hybrid cache in program.cs

```` csharp
builder.Services.AddStackExchangeRedisCache(
    options =>
    {
        options.Configuration =
            builder.Configuration
                .GetConnectionString("redis");
    });
````

## Add Tooling for Redis viewing

Update the redis line to be

```` csharp
var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithRedisInsight();
````

# Run the Application

No real changes, but notice startup differences.

* Look at the tools
* Consider which you like better

## Create a helper for our Dashboard Summary Object

To make things easier if we have no dashboard, we can setup a empty state.  Modify `DashboardSummary.cs` with the following.

```` csharp
namespace ProjectHub.Application.Dashboard;

public sealed record DashboardSummary(
    int ProjectCount,
    int OpenWorkItems,
    int AssignedToMe,
    int OverdueWorkItems,
    int CompletedThisWeek)
{
    public static DashboardSummary Empty { get; } = new DashboardSummary(0, 0, 0, 0, 0);
}
````

## Caching the Dashboard

We want to update the process to avoid SQL queries every time we hit the dashboard, so lets start with a re-implementation of the Dashboard service

```` csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using ProjectHub.Application.Identity;
using ProjectHub.Data;
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Dashboard;

[RegisterScoped]
internal sealed class DashboardService(IDbContextFactory<ApplicationDbContext> contextFactory, ICurrentUser currentUser, HybridCache cache)
    : IDashboardService
{
    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync();

        if (!userId.HasValue)
        {
            return DashboardSummary.Empty;
        }

        return await cache.GetOrCreateAsync(
            GetCacheKey(userId.Value),
            async cancel =>
            {
                return await BuildSummaryAsync(
                    userId.Value,
                    cancel);
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),

                LocalCacheExpiration = TimeSpan.FromSeconds(30)
            },
            tags:
            [
                "dashboard",
                GetUserTag(userId.Value)
            ],
            cancellationToken:
                cancellationToken);
    }

    private async Task<DashboardSummary> BuildSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await using var db =
            await contextFactory.CreateDbContextAsync(
                cancellationToken);

        var now = DateTime.UtcNow;

        var startOfWeek =
            now.Date.AddDays(
                -(int)now.DayOfWeek);

        var projectCount =
            await db.ProjectMembers
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId)
                .Select(x =>
                    x.ProjectId)
                .Distinct()
                .CountAsync(
                    cancellationToken);

        var stats =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.Project.Members.Any(
                        m => m.UserId == userId))
                .GroupBy(_ => 1)
                .Select(x => new
                {
                    Open =
                        x.Count(w =>
                            w.Status !=
                                WorkItemStatus.Completed),

                    Assigned =
                        x.Count(w =>
                            w.AssignedToUserId == userId &&
                            w.Status !=
                                WorkItemStatus.Completed),

                    Overdue =
                        x.Count(w =>
                            w.Status !=
                                WorkItemStatus.Completed &&
                            w.DueDateUtc != null &&
                            w.DueDateUtc < now),

                    CompletedThisWeek =
                        x.Count(w =>
                            w.Status ==
                                WorkItemStatus.Completed &&
                            w.UpdatedOnUtc >=
                                startOfWeek)
                })
                .SingleOrDefaultAsync(
                    cancellationToken);

        return new DashboardSummary(
            projectCount,
            stats?.Open ?? 0,
            stats?.Assigned ?? 0,
            stats?.Overdue ?? 0,
            stats?.CompletedThisWeek ?? 0);
    }

    private static string GetCacheKey(Guid userId)
        => $"dashboard:{userId:N}";

    private static string GetUserTag(Guid userId)
        => $"dashboard-user:{userId:N}";
}
````


