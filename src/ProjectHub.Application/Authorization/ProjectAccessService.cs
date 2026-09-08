using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using ProjectHub.Data;
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Authorization;

[RegisterScoped]
internal sealed class ProjectAccessService(IDbContextFactory<ApplicationDbContext> contextFactory, HybridCache cache)
    : IProjectAccessService
{
    public async Task<ProjectAccessEntry?> GetAccessAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var access = await GetUserAccessAsync(userId, cancellationToken);

        return access.FirstOrDefault(x => x.ProjectId == projectId);
    }

    public async Task<bool> IsMemberAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var access = await GetAccessAsync(userId, projectId, cancellationToken);

        return access != null;
    }

    public async Task<bool> IsManagerAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var access = await GetAccessAsync(userId, projectId, cancellationToken); ;

        return access?.Role == ProjectMemberRole.ProjectManager;
    }

    public ValueTask InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(GetCacheKey(userId), cancellationToken);
    }

    private async Task<ProjectAccessEntry[]> GetUserAccessAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            GetCacheKey(userId),
            async cancel =>
            {
                await using var db =
                    await contextFactory
                        .CreateDbContextAsync(cancel);

                return await db.ProjectMembers
                    .AsNoTracking()
                    .Where(x => x.UserId == userId)
                    .Select(x =>
                        new ProjectAccessEntry(
                            x.ProjectId,
                            x.Role))
                    .ToArrayAsync(cancel);
            },
            cancellationToken: cancellationToken);
    }

    private static string GetCacheKey(Guid userId)
        => $"project-access:{userId:N}";
}