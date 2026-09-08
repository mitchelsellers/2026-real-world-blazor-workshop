namespace ProjectHub.Application.Authorization;

/// <summary>
/// Contains the names of the authorization policies used in the ProjectHub application. 
/// These policy names are used to configure authorization requirements and can be referenced throughout 
/// the application to enforce access control based on user roles and permissions.
/// </summary>
public static class ProjectHubPolicyNames
{
    /// <summary>
    /// The policy name for the ProjectMember requirement. This is used in the authorization configuration to define a policy that requires the user to be a project member.
    /// </summary>
    public static string ProjectMember => "ProjectMember";
    /// <summary>
    /// The policy name for the ProjectManager requirement. This is used in the authorization configuration to define a policy that requires the user to be a project manager.
    /// </summary>
    public static string ProjectManager => "ProjectManager";
}
