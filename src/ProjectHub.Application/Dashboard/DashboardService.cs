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