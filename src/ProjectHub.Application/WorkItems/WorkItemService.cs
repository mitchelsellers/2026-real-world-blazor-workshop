using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Dashboard;
using ProjectHub.Data;
using ProjectHub.Data.Models;

namespace ProjectHub.Application.WorkItems;

[RegisterScoped]
internal sealed class WorkItemService(IDbContextFactory<ApplicationDbContext> contextFactory, IDashboardCacheInvalidator dashboardCacheInvalidator)
    : IWorkItemService
{
    public async Task<Guid> CreateAsync(CreateWorkItemRequest request, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        //Validate that the project exists before creating a work item for it
        var projectExists = await db.Projects.AnyAsync(x => x.ProjectId == request.ProjectId, cancellationToken);

        if (!projectExists)
        {
            throw new InvalidOperationException("Project was not found.");
        }

        var item = new WorkItem
        {
            ProjectId = request.ProjectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            DueDateUtc = request.DueDateUtc,
            Status = WorkItemStatus.New
        };

        db.WorkItems.Add(item);

        await db.SaveChangesAsync(cancellationToken);

        //For now, clear everyone, but lets talk about optimizations
        await dashboardCacheInvalidator.InvalidateAllDashboard(cancellationToken);

        return item.WorkItemId;
    }
}