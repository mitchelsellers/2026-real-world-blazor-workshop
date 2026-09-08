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