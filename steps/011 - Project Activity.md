# Project Activity

Lets get a bit more information available for us and look at how we can query it safely

## ProjectActivity.cs

Add to the /Projects folder in the Application project

```` csharp
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Projects;

public sealed record ProjectActivityView(
    Guid ProjectId,
    string ProjectName,
    int MemberCount,
    int WorkItemCount,
    IReadOnlyList<ProjectActivityWorkItem> WorkItems);

public sealed record ProjectActivityWorkItem(
    Guid WorkItemId,
    string Title,
    WorkItemStatus Status,
    int CommentCount);
````

## update IProjectService.cs

Add the following extra bits

```` csharp
/// <summary>
/// Gets the activity of a specific project by its ID in an async manner
/// </summary>
/// <param name="projectId">The ID of the project to retrieve activity for</param>
/// <param name="cancellationToken">A cancellation token to abort if needed</param>
/// <returns>The activity of the specified project, or null if not found</returns>
Task<ProjectActivityView?> GetProjectActivityAsync(Guid projectId, CancellationToken cancellationToken = default);
````

## Update ProjectService.cs with Impelementation

Add the following

```` csharp
public async Task<ProjectActivityView?> GetProjectActivityAsync(Guid projectId, CancellationToken cancellationToken = default)
{
    await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

    return await db.Projects
        .AsNoTracking()
        .Where(x => x.ProjectId == projectId)
        .Select(x => new ProjectActivityView(
            x.ProjectId,
            x.Name,
            x.Members.Count,
            x.WorkItems.Count,
            x.WorkItems
                .OrderByDescending(w => w.CreatedOnUtc)
                .Select(w => new ProjectActivityWorkItem(
                    w.WorkItemId,
                    w.Title,
                    w.Status,
                    w.Comments.Count))
                .ToList()))
        .SingleOrDefaultAsync(cancellationToken);
}
````

## Create a LInk to Our New Page

Update the `Detail.razor` page to have the following link.  Can add wherever.

```` html
<p>
    <a href="@($"/projects/{ProjectId}/activity")"
        class="btn btn-outline-secondary">
        View Activity
    </a>
</p>
````

## Create Activity.razor within /Projects

This is our page to view it

```` razor
@page "/projects/{ProjectId:guid}/activity"
@rendermode InteractiveServer

@using ProjectHub.Application.Projects

@inject IProjectService ProjectService

<PageTitle>Project Activity</PageTitle>

<div class="container py-4">

    @if (_project == null)
    {
        <p>Loading...</p>
    }
    else
    {
        <div class="mb-4">
            <a href="@($"/projects/{ProjectId}")"
               class="text-decoration-none">
                &larr; Back to Project
            </a>
        </div>

        <h1>@_project.ProjectName Activity</h1>

        <div class="row my-4">
            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5>Members</h5>
                        <div class="display-6">
                            @_project.MemberCount
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5>Work Items</h5>
                        <div class="display-6">
                            @_project.WorkItems.Count
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5>Comments</h5>
                        <div class="display-6">
                            @_project.WorkItems.Sum(x => x.CommentCount)
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <h2>Work Item Activity</h2>

        <table class="table">
            <thead>
                <tr>
                    <th>Work Item</th>
                    <th>Status</th>
                    <th>Comments</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var item in _project.WorkItems)
                {
                    <tr>
                        <td>@item.Title</td>
                        <td>@item.Status</td>
                        <td>@item.CommentCount</td>
                    </tr>
                }
            </tbody>
        </table>
    }

</div>

@code {
    [Parameter]
    public Guid ProjectId { get; set; }

    private ProjectActivityView? _project;

    protected override async Task OnParametersSetAsync()
    {
        _project = await ProjectService.GetProjectActivityAsync(ProjectId);
    }
}
````

## Discussion Points

* Where are we now?
* What considerations are next?