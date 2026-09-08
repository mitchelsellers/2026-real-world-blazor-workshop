namespace ProjectHub.Application.Authorization;

/// <summary>
/// This is a service to gate access to project resources. It is used to determine if a user has access to a project and what role they have in that project.
/// </summary>
public interface IProjectAccessService
{
    Task<ProjectAccessEntry?> GetAccessAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    Task<bool> IsMemberAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    Task<bool> IsManagerAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    ValueTask InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default);
}