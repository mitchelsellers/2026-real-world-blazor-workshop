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