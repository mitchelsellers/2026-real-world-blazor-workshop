# Initial UI Creations

Lets start with creating some UI elements, not worrying about the security side of things quite yet!

## Create the Project Listing Page

Grouping is all about how you will be able to find things.  Some folks like to do `/Components/Pages/{SECTION}` to group.  I personally pefer `/Components/{Section}/` to help group things at a higher, level.

Let's start off easy.

* Add a folder  under `Components` in the We project called `Projects`
* Right click on the folder and select "Add" -> "Razor Component"
* Name it `Index.razor` and replace the content with the below

```` razor
@page "/projects"
@rendermode InteractiveServer

@using Microsoft.EntityFrameworkCore
@using ProjectHub.Data
@using ProjectHub.Data.Models

@inject IDbContextFactory<ApplicationDbContext> ContextFactory

<!-- Discussion Point: What this Does -->
<PageTitle>Projects</PageTitle>

<h1>Projects</h1>

@if (_projects == null)
{
    <p>Loading...</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Project</th>
                <th>Status</th>
                <th>Open Items</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var project in _projects)
            {
                <tr>
                    <td>@project.Name</td>
                    <td>@project.Status</td>
                    <td>
                        @project.WorkItems.Count(
                            x => x.Status !=
                                WorkItemStatus.Completed)
                    </td>
                </tr>
            }
        </tbody>
    </table>
}

@code
{
    private List<Project>? _projects;

    protected override async Task OnInitializedAsync()
    {
        await using var db = await ContextFactory.CreateDbContextAsync();

        _projects = await db.Projects
            .Include(x => x.WorkItems)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }
}
````

## Add a Menu Item to Make it Easy

Edit the `Components\Layout\NavMenu.razor` file and add a new mtenu item after the home, using the following code.

```` razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="/projects">
        <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Projects
    </NavLink>
</div>
````

## Discussion Items at this point

1. How much data do we actually query?
2. Data from UI & Maintainability?
3. Lets Review OpenTelemetry & Logging with Default Configurations

