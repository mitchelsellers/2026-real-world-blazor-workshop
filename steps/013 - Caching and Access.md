# Addition of Caching and Tooling for Future use

The goal of this step is to start to add caching, at a basic level so we can start to secure items in a manner that will work well.

## Add Hybrid Cache to the Projects

We will use the HybridCache as it supports both in-memory and distributed cache for when we need/want to use it.

We are going to add it to both the `Application` and `Web` projects so that we can proper register & use.

```` powershell
Install-Package Microsoft.Extensions.Caching.Hybrid
````

## Add Service Registration to the Web Program.cs

Add the following at the bottom of the builder process to do initial default configurations, setting defaults for short cache durations.

```` csharp
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions =
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(2),
            LocalCacheExpiration = TimeSpan.FromMinutes(2)
        };
});
````

## Lets Create a Service using it

The goal of this service is to get a user's collection of available projects and do so with a quick and easy interface.

To help keep things clear we will create a new `Authorization` folder within the `Application` project and add the following items

### ProjectAccessEntry.cs

```` csharp
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Authorization;

public sealed record ProjectAccessEntry(
    Guid ProjectId,
    ProjectMemberRole Role);
````

### IProjectAccessService.cs

```` csharp
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
````

### ProjectAccessService.cs

```` csharp
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
````

## Create an Authorization Handler to Use

The idea here is that we can create an AUthorization Handler to use this process to then force security within the system.

Create the following files in the /Authorization folder

### ProjectMemberRequirement.cs

```` csharp
using Microsoft.AspNetCore.Authorization;

namespace ProjectHub.Application.Authorization;

/// <summary>
/// Used to set a requirement that a user is a member of a project
/// </summary>
public sealed class ProjectMemberRequirement
    : IAuthorizationRequirement;
````

### ProjectAuthorizationResource.cs

The resource we want to secure.

```` csharp
namespace ProjectHub.Application.Authorization;

public sealed record ProjectAuthorizationResource(Guid ProjectId);
````

### ProjectMemberAuthorizationHandler.cs

```` csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProjectHub.Application.Authorization;

[RegisterScoped<IAuthorizationHandler>]
internal sealed class ProjectMemberAuthorizationHandler(
    IProjectAccessService projectAccessService)
    : AuthorizationHandler<
        ProjectMemberRequirement,
        ProjectAuthorizationResource>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectMemberRequirement requirement,
        ProjectAuthorizationResource resource)
    {
        var userIdValue =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return;
        }

        if (await projectAccessService.IsMemberAsync(
            userId,
            resource.ProjectId))
        {
            context.Succeed(requirement);
        }
    }
}
````

## Modify Program.cs

Update Program.cs in the web project to allow this to be used.  We enable authorization and register the policy with a set name.  Add teh following after authentication.

```` csharp
builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(
            "ProjectMember",
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ProjectMemberRequirement());
            });
    });
````

>[!NOTE]
>Order matters here!

## Wire up usage

Given that we haven't do anything within our code yet to setup permissions, it will fail, but we want to demo it.

We will make these changes to the `Detail.razor` file within the projects.

Add the following two injections at the top, in addition to the existing

```` csharp
@inject IAuthorizationService AuthorizationService
@inject AuthenticationStateProvider AuthenticationStateProvider
````

Replace the code within the null check with the following

```` html
@if (_project == null)
{
	if (_accessDenied)
	{
		<p class="text-danger">
			Access Denied. You do not have permission to view this project.
		</p>
	}
	else
	{
		<p>Loading...</p>
	}
}
````

Lastly replace the code section with the following

```` csharp
@code
{
	[Parameter]
	public Guid ProjectId { get; set; }

	private ProjectDetail? _project;
	private bool _accessDenied;

	protected override async Task OnParametersSetAsync()
	{
		var authState = await AuthenticationStateProvider
					.GetAuthenticationStateAsync();

		var resource = new ProjectAuthorizationResource(ProjectId);

		var authorization = await AuthorizationService.AuthorizeAsync(authState.User, resource, "ProjectMember");

		if (!authorization.Succeeded)
		{
			_accessDenied = true;
			return;
		}

		_project = await ProjectService.GetProjectAsync(ProjectId);
	}
}
````


# Discussion Topics

* Magic Strings = What do do?  "ProjectMember"
* Authorization Service vs. DB Call?  What about "GetProjectAsync()" 
* Expansion to additional polcies?  Such as Project Manager, etc?


# Bonus Items (Policy that Queries the DB)

## ProjectManagerRequirement

```` csharp
using Microsoft.AspNetCore.Authorization;

namespace ProjectHub.Application.Authorization;

public sealed class ProjectManagerRequirement
    : IAuthorizationRequirement;
````

## ProjectManagerAuthorizationHandler

```` csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Data.Models;
using System.Security.Claims;

namespace ProjectHub.Application.Authorization;

[RegisterScoped<IAuthorizationHandler>]
internal sealed class ProjectManagerAuthorizationHandler(
    IDbContextFactory<ApplicationDbContext> contextFactory)
    : AuthorizationHandler<
        ProjectManagerRequirement,
        ProjectAuthorizationResource>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectManagerRequirement requirement,
        ProjectAuthorizationResource resource)
    {
        var userIdValue =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        using var db = await contextFactory.CreateDbContextAsync();

        var allowed =
            await db.ProjectMembers
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ProjectId == resource.ProjectId &&
                    x.Role == ProjectMemberRole.ProjectManager &&
                    x.UserId == userId);

        if (allowed)
        {
            context.Succeed(requirement);
        }
    }
}
````

## Additions to Program.cs

```` csharp
options.AddPolicy(
    "ProjectManager",
    policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new ProjectManagerRequirement());
    });
````








