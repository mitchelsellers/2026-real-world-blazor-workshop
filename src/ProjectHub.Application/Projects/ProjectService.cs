using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Projects;

[RegisterScoped]
internal sealed class ProjectService(IDbContextFactory<ApplicationDbContext> contextFactory)
    : IProjectService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ProjectListItem>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Projects
            .AsNoTracking() // No need to track changes for read-only operations
            .OrderBy(x => x.Name)
            .Select(x => new ProjectListItem(
                x.ProjectId,
                x.Name,
                x.Status,
                x.WorkItems.Count(
                    w => w.Status !=
                        WorkItemStatus.Completed)))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProjectDetail?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Projects
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .Select(x => new ProjectDetail(
                x.ProjectId,
                x.Name,
                x.Description,
                x.Status,
                x.WorkItems
                    .OrderByDescending(w => w.Priority)
                    .ThenBy(w => w.DueDateUtc)
                    .Select(w => new ProjectWorkItem(
                        w.WorkItemId,
                        w.Title,
                        w.Status,
                        w.Priority,
                        w.AssignedToUserId,
                        w.DueDateUtc,
                        w.Comments.Count))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}