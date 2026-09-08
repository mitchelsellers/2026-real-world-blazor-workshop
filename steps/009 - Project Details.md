# Creation of Project Details

## Addition to `IProjectService`

Add a new method defintiion to our project service.

```` csharp
    /// <summary>
    /// Gets the details of a specific project by its ID in an async manner
    /// </summary>
    /// <param name="projectId">The ID of the project to retrieve</param>
    /// <param name="cancellationToken">A cancellation token to abort if needed</param>
    /// <returns>The details of the specified project, or null if not found</returns>
    Task<ProjectDetail?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
````

## New Project Detail Model

Add a new file `ProjectDetail.cs` with the following content for the DTO.

```` csharp
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Projects;

public sealed record ProjectDetail(
    Guid ProjectId,
    string Name,
    string? Description,
    ProjectStatus Status,
    IReadOnlyList<ProjectWorkItem> WorkItems);

public sealed record ProjectWorkItem(
    Guid WorkItemId,
    string Title,
    WorkItemStatus Status,
    WorkItemPriority Priority,
    Guid? AssignedToUserId,
    DateTime? DueDateUtc,
    int CommentCount);
````

## Implement the new Method

Add to the existing `ProjectService.cs` the following implementation

```` csharp
/// <inheritdoc />
public async Task<ProjectDetail?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
{
    await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

    return await db.Projects
        .AsNoTracking()
        .Where(x => x.ProjectId == projectId)
        .Select(x => new ProjectDetail(
            x.ProjectId,
            x.Name,
            x.Description,
            x.Status,
            x.WorkItems
                .OrderByDescending(w => w.Priority)
                .ThenBy(w => w.DueDateUtc)
                .Select(w => new ProjectWorkItem(
                    w.WorkItemId,
                    w.Title,
                    w.Status,
                    w.Priority,
                    w.AssignedToUserId,
                    w.DueDateUtc,
                    w.Comments.Count))
                .ToList()))
        .SingleOrDefaultAsync(cancellationToken);
}
````

## Modify Project List Page

Update the `<td>@project.Name</td>` table cell to be.

```` razor
<td>
    <a href="@($"/projects/{project.ProjectId}")">
        @project.Name
    </a>
</td>
````

## Add a new `Detail` Page

Add a new razor component to the projects folder with the name `Detail.razor` and place the following content.

```` razor
@page "/projects/{ProjectId:guid}"

@rendermode InteractiveServer

@using ProjectHub.Application.Projects

@inject IProjectService ProjectService

<PageTitle>Project</PageTitle>

@if (_project == null)
{
    <p>Project not found.</p>
}
else
{
    <h1>@_project.Name</h1>

    <p>@_project.Description</p>

    <div class="mb-3">
        <span class="badge bg-secondary">
            @_project.Status
        </span>
    </div>

    <h2>Work Items</h2>

    <table class="table">
        <thead>
            <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Due</th>
                <th>Comments</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in _project.WorkItems)
            {
                <tr>
                    <td>@item.Title</td>
                    <td>@item.Status</td>
                    <td>@item.Priority</td>
                    <td>
                        @item.DueDateUtc?.ToString("d")
                    </td>
                    <td>@item.CommentCount</td>
                </tr>
            }
        </tbody>
    </table>
}

@code
{
    [Parameter]
    public Guid ProjectId { get; set; }

    private ProjectDetail? _project;

    protected override async Task OnParametersSetAsync()
    {
        _project = await ProjectService.GetProjectAsync(ProjectId);
    }
}
````

## Discussion Points

* How do our namespaces work now?
* Would you want more organization?
* EF Core Side-Quest Note - Single vs. Split Query
* What happens as we navigate page-to-page