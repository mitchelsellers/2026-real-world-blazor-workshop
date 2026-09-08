namespace ProjectHub.Application.Projects;

public interface IProjectService
{
    /// <summary>
    /// Gets a listing of all projects in an async manner
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to abort if needed</param>
    /// <returns>A read-only list of project list items</returns>
    Task<IReadOnlyList<ProjectListItem>> GetProjectsAsync(CancellationToken cancellationToken = default);
}