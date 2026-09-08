# Creation of an Application Layer

There are many options here, but we will go with a simple layer for now.  Web -> Application -> Data.  You may utilize a larger or more complex setup if desired.

## Create Project & Add References

* Create a new Class Library `ProjectHub.Application`
* Delete the default created `Class1.cs`
* Add a reference to `ProjectHub.Data`
* Add a reference to `ProjectHub.Application` from `ProjectHub.Web`

>[!Note]
>We do need to leave the direct link between Web -> Data for the support of EFMigrations as well as the Authentication stuff created by Identity

## Create a Simple DTO Object & Service Interface

The goal here is to avoid sharing the DB Model directly to the UI.

* Create a new folder `Projects` within the `ProjectHub.Application` project
* Add a new file `ProjectListItem.cs` using the content below to create a simple DTO record object (We can change to a class later if needed)
* Add a new file `IProjectService.cs` to store our interface that will be used by the project

ProjectListItem.cs
```` csharp
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Projects;

public sealed record ProjectListItem(
    Guid ProjectId,
    string Name,
    ProjectStatus Status,
    int OpenWorkItemCount);
````

IProjectService.cs
```` csharp
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
````

## Create a Concrete Implementation of the Service

Now we want to implement a process to call/use this service

Create a new file `ProjectService.cs` and replace with the below contents.

```` csharp
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Projects;

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
````

## Register for Dependency Injection

We can do this the easy way and use the various .AddScoped<> and similar methods.

But thee are easier ways to handle this

Using the Package Mangaer console install the following package to the `ProjectHub.Application` project.

```` powershell
Install-Package Injectio
````

Now, modify your `ProjectService.cs` file and add the below attribute to the class.

```` csharp
[RegisterScoped]
````

Lastly, add the following line to `Program.cs` within the `ProjectHub.Web` project.

```` csharp
builder.Services.AddProjectHubApplication();
````

This helps to create a bit more convention, rather than static registration of events, we will talk more about this in a bit!

## Update the UI to use our new service

Within th `Index.razor` that was added for the Projects we will make two changes.

### Update the Top Section

At the top section replace the existing `using` and `@inject` statements with the following

```` csharp
@using ProjectHub.Data.Models
@using ProjectHub.Application.Projects

@inject IProjectService ProjectService
````

### Update the Code Section

Update the code section to the following

```` csharp
@code
{
    private IReadOnlyList<ProjectListItem>? _projects;

    protected override async Task OnInitializedAsync()
    {
		_projects = await ProjectService.GetProjectsAsync();
    }
}
````

### Update the Table

Lastly, now that we have a properly named field `OpenWorkItemCount` we should update the third column to use that property rather than a calculation!


## Discussion Items

* What changed now!  Lets look at the SQL
* Is this a proper Application Object?
* What might be a breaking change here?
* Do we fully separate things?
* Injectio vs. Normal
* Transient/Scoped What?