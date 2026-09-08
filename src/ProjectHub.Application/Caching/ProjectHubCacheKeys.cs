namespace ProjectHub.Application.Caching;

/// <summary>
/// Centralized collection of cache keys to avoid magic strings.
/// </summary>
internal class ProjectHubCacheKeys
{
    /// <summary>
    /// Generic dashboard tag, which is added to all items that are dashboard related
    /// </summary>
    public static string DashboardTag => "dashboard";
    /// <summary>
    /// Creates a user specific dashboard tag, which is added to all items that are related to a specific user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static string DashboardUserTag(Guid userId)
        => $"dashboard-user:{userId:N}";
    /// <summary>
    /// Creates a project specific tag, which is added to all items that are related to a specific project
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public static string ProjectTag(Guid projectId)
        => $"project:{projectId:N}";
    /// <summary>
    /// Creates a user specific project access tag, which is added to all items that are related to a specific user's access to projects
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static string ProjectAccess(Guid userId)
        => $"project-access:{userId:N}";
}
