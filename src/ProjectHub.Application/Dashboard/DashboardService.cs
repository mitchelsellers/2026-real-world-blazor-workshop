using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Identity;
using ProjectHub.Data;
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Dashboard;

[RegisterScoped]
internal sealed class DashboardService(IDbContextFactory<ApplicationDbContext> contextFactory, ICurrentUser currentUser)
    : IDashboardService
{
    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync();

        if (!userId.HasValue)
        {
            return new DashboardSummary(0, 0, 0, 0, 0);
        }

        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);

        var stats =  await db.WorkItems
            .AsNoTracking()
            .Where(x =>
                x.Project.Members.Any(
                    m => m.UserId == userId.Value))
            .GroupBy(_ => 1)
            .Select(x => new
            {
                Open =
                    x.Count(w =>
                        w.Status != WorkItemStatus.Completed),

                Assigned =
                    x.Count(w =>
                        w.AssignedToUserId == userId.Value &&
                        w.Status != WorkItemStatus.Completed),

                Overdue =
                    x.Count(w =>
                        w.Status != WorkItemStatus.Completed &&
                        w.DueDateUtc != null &&
                        w.DueDateUtc < now),

                CompletedThisWeek =
                    x.Count(w =>
                        w.Status == WorkItemStatus.Completed &&
                        w.UpdatedOnUtc >= startOfWeek)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var projectCount = await db.ProjectMembers
            .AsNoTracking()
            .Where(x => x.UserId == userId.Value)
            .Select(x => x.ProjectId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new DashboardSummary(
                    projectCount,
                    stats?.Open ?? 0,
                    stats?.Assigned ?? 0,
                    stats?.Overdue ?? 0,
                    stats?.CompletedThisWeek ?? 0);
    }
}