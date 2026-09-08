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

        // NOTE: This is ugly on purpose!
        var projectCount =
            await db.ProjectMembers
                .AsNoTracking()
                .Where(x => x.UserId == userId.Value)
                .Select(x => x.ProjectId)
                .Distinct()
                .CountAsync(cancellationToken);

        var openWorkItems =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.Project.Members.Any(
                        m => m.UserId == userId.Value) &&
                    x.Status != WorkItemStatus.Completed)
                .CountAsync(cancellationToken);

        var assignedToMe =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.AssignedToUserId == userId.Value &&
                    x.Status != WorkItemStatus.Completed)
                .CountAsync(cancellationToken);

        var overdue =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.Project.Members.Any(
                        m => m.UserId == userId.Value) &&
                    x.Status != WorkItemStatus.Completed &&
                    x.DueDateUtc != null &&
                    x.DueDateUtc < now)
                .CountAsync(cancellationToken);

        var completedThisWeek =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.Project.Members.Any(
                        m => m.UserId == userId.Value) &&
                    x.Status == WorkItemStatus.Completed &&
                    x.UpdatedOnUtc >= startOfWeek)
                .CountAsync(cancellationToken);

        return new DashboardSummary(
            projectCount,
            openWorkItems,
            assignedToMe,
            overdue,
            completedThisWeek);
    }
}