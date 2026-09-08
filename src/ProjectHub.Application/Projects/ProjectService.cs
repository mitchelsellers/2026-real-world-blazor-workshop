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
}