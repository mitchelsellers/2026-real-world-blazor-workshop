# Creating a Cache Invalidator

Centralizing and controlling cache access/updates is key to the long-term success of the project.  To help with this, lets create a process to systematically control invalidation and let the appliation manage properly.

If we run the appliation right now and view the dashboard, then add a new item, we may see incorrect results, this is due to the overall cache hit/miss process. 

Lets fix that!

## Create DashboardCacheInvalidator.cs

Create this within the `/Dashboard` or `/Cache` folders within the Application project.

First the Interface

```` csharp
namespace ProjectHub.Application.Dashboard;

/// <summary>
/// This service is used to invalidate dashboard related cache entries. It is used to ensure that when a user makes changes to their projects, the dashboard cache is invalidated and the user sees the most up to date information.
/// </summary>
public interface IDashboardCacheInvalidator
{

    ValueTask InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default);

    ValueTask InvalidateUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    ValueTask InvalidateAllDashboard(CancellationToken cancellationToken = default);
}
````

Then the implementation

```` csharp
using Microsoft.Extensions.Caching.Hybrid;
using ProjectHub.Application.Caching;

namespace ProjectHub.Application.Dashboard;

[RegisterScoped]
internal sealed class DashboardCacheInvalidator(HybridCache cache)
    : IDashboardCacheInvalidator
{
    public ValueTask InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return cache.RemoveByTagAsync(ProjectHubCacheKeys.DashboardUserTag(userId), cancellationToken);
    }

    public async ValueTask InvalidateUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        // Never trust incoming stuff, so we will distinct the userIds to avoid duplicate invalidation
        foreach (var userId in userIds.Distinct())
        {
            await InvalidateUserAsync(userId, cancellationToken);
        }
    }

    public async ValueTask InvalidateAllDashboard(CancellationToken cancellationToken = default)
    {
        // We are able to clear ALL this way, because we have a proper tag that is added to all items
        await cache.RemoveByTagAsync(ProjectHubCacheKeys.DashboardTag, cancellationToken);
    }
}
````

## Update New Work Item

Now when creating a work item, invalidate the cache!

* Add `IDashboardCacheInvalidator` to the injected services
* Add `await dashboardCacheInvalidator.InvalidateAllDashboard(cancellationToken);` after the `db.SaveChangesAsync` call


# Discussion Points

* Importance of dual tagging a cache key?
* Performance implications of cache clearing?
* How do I better target the clearing of the cache?