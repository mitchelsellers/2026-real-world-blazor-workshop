namespace ProjectHub.Application.Projects;

public interface IProjectService
{
    /// <summary>
    /// Gets a listing of all projects in an async manner
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to abort if needed</param>
    /// <returns>A read-only list of project list items</returns>
    Task<IReadOnlyList<ProjectListItem>> GetProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the details of a specific project by its ID in an async manner
    /// </summary>
    /// <param name="projectId">The ID of the project to retrieve</param>
    /// <param name="cancellationToken">A cancellation token to abort if needed</param>
    /// <returns>The details of the specified project, or null if not found</returns>
    Task<ProjectDetail?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the activity of a specific project by its ID in an async manner
    /// </summary>
    /// <param name="projectId">The ID of the project to retrieve activity for</param>
    /// <param name="cancellationToken">A cancellation token to abort if needed</param>
    /// <returns>The activity of the specified project, or null if not found</returns>
    Task<ProjectActivityView?> GetProjectActivityAsync(Guid projectId, CancellationToken cancellationToken = default);
}