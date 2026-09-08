# Cleaning Up Magic Strings

Now that we have some patterns emerging, we need to try to cleanup a few things so we don't have lots of duplication.

##  Policy Names 

Create a new file `ProjectHub.Application\Authorization\ProjectHubPolicyNames.cs`

This is a static class to contain the names of the policies to avoid needing magic strings.

```` csharp
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
````

### Update internal references

Update the Authorization code within `Program.cs` for the Web project

```` csharp
builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(
            ProjectHubPolicyNames.ProjectMember,
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ProjectMemberRequirement());
            });

        options.AddPolicy(
            ProjectHubPolicyNames.ProjectManager,
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ProjectManagerRequirement());
            });
    });
````

Update the Authorization call in `Components\Projects\Detail.razor` to use the new value

>[!NOTE]
>This is public becuase we have the desire/need to use this from the web

```` csharp
var authorization = await AuthorizationService.AuthorizeAsync(authState.User, resource, ProjectHubPolicyNames.ProjectMember);
````

## Cache Keys

Create a new folder and file `ProjectHub.Application\Cache\ProjectHubCacheKeys.cs`

This is going to contain the cache keys, or cache-key helpers for us so that we can work to better manage cache keys.

```` csharp
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

````

>[!NOTE]
>This is purposfully `internal` as we DO NOT want others to use it!

### Update Existing Usages

* Update `ProjectAccessService.cs` replacing `GetCacheKey(userId)` with `ProjectHubCacheKeys.ProjectAccess(userId)` in all locations and remove the internal `GetCacheKey` method
* Update `DashboardService.cs` Replacing the existing `GetCacheKey` method with `ProjectHubCacheKeys.DashboardUserTag`, and also `GetUserTag` with the same value, and then remove the internal `Get` methods.
* Update `DashboardService.cs` Replacing the hard-coded "dashboard" string with `ProjectHubCacheKeys.DashboardTag`


# Discussion Points

* Find All References & Other Benefits of this
* Drawbacks of this approach?


* 