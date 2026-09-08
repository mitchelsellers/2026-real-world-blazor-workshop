# My Projects Dashboard

This is a starting point of giving users more access to the DB, and will help us to get to a point of better understanding for the overall caching & implementation.

## Setup Permissions

Given that we haven't done much yet to grant access you can use the following SQL Query to ensure that all users have Project Manager access to all Projects

```` sql
INSERT INTO ProjectMembers
    (ProjectMemberId, ProjectId, UserId, Role, AddedOnUtc)
SELECT NewId(), ProjectId, Id, 2, GETUTCDATE()
FROM Projects
    CROSS APPLY AspNetUsers
````

## Create Dashboard Elements

### Application/Dashboard/DashboardSummary.cs

This is the model for the summary

```` csharp
namespace ProjectHub.Application.Dashboard;

public sealed record DashboardSummary(
    int ProjectCount,
    int OpenWorkItems,
    int AssignedToMe,
    int OverdueWorkItems,
    int CompletedThisWeek);
````

### Application/Dashboard/IDashboardService.cs

This is the service to get it

```` csharp
namespace ProjectHub.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
}
````

### Application/Dashboard/DashboardService.cs

This is a baseline implementation with a few key "oh no" moments in it on purpose.

```` csharp
using Microsoft.EntityFrameworkCore;
using ProjectHub.Application.Identity;
using ProjectHub.Data;
using ProjectHub.Data.Models;

namespace ProjectHub.Application.Dashboard;

[RegisterScoped]
internal sealed class DashboardService(IDbContextFactory<ApplicationDbContext> contextFactory, ICurrentUser currentUser)
    : IDashboardService
{
    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync();

        if (!userId.HasValue)
        {
            return new DashboardSummary(0, 0, 0, 0, 0);
        }

        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);

        // NOTE: This is ugly on purpose!
        var projectCount =
            await db.ProjectMembers
                .AsNoTracking()
                .Where(x => x.UserId == userId.Value)
                .Select(x => x.ProjectId)
                .Distinct()
                .CountAsync(cancellationToken);

        var openWorkItems =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.Project.Members.Any(
                        m => m.UserId == userId.Value) &&
                    x.Status != WorkItemStatus.Completed)
                .CountAsync(cancellationToken);

        var assignedToMe =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.AssignedToUserId == userId.Value &&
                    x.Status != WorkItemStatus.Completed)
                .CountAsync(cancellationToken);

        var overdue =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.Project.Members.Any(
                        m => m.UserId == userId.Value) &&
                    x.Status != WorkItemStatus.Completed &&
                    x.DueDateUtc != null &&
                    x.DueDateUtc < now)
                .CountAsync(cancellationToken);

        var completedThisWeek =
            await db.WorkItems
                .AsNoTracking()
                .Where(x =>
                    x.Project.Members.Any(
                        m => m.UserId == userId.Value) &&
                    x.Status == WorkItemStatus.Completed &&
                    x.UpdatedOnUtc >= startOfWeek)
                .CountAsync(cancellationToken);

        return new DashboardSummary(
            projectCount,
            openWorkItems,
            assignedToMe,
            overdue,
            completedThisWeek);
    }
}
````

## Add Web/Components/Pages/Dashboard.razor

NOTE: When initially adding this it will error, do the next step to clear the issues

```` html
@page "/dashboard"

@rendermode InteractiveServer

@attribute [Authorize]

@using ProjectHub.Application.Dashboard

@inject IDashboardService DashboardService

<PageTitle>Dashboard</PageTitle>

<div class="container py-4">

    <div class="mb-4">
        <h1>My Dashboard</h1>

        <p class="text-muted">
            A quick view of your projects and work.
        </p>
    </div>

    @if (_summary == null)
    {
        <p>Loading...</p>
    }
    else
    {
        <div class="row g-3">

            <div class="col-md-4 col-xl">
                <DashboardCard
                    Title="My Projects"
                    Value="@_summary.ProjectCount" />
            </div>

            <div class="col-md-4 col-xl">
                <DashboardCard
                    Title="Open Work Items"
                    Value="@_summary.OpenWorkItems" />
            </div>

            <div class="col-md-4 col-xl">
                <DashboardCard
                    Title="Assigned to Me"
                    Value="@_summary.AssignedToMe" />
            </div>

            <div class="col-md-4 col-xl">
                <DashboardCard
                    Title="Overdue"
                    Value="@_summary.OverdueWorkItems" />
            </div>

            <div class="col-md-4 col-xl">
                <DashboardCard
                    Title="Completed This Week"
                    Value="@_summary.CompletedThisWeek" />
            </div>

        </div>
    }

</div>

@code {
    private DashboardSummary? _summary;

    protected override async Task OnInitializedAsync()
    {
        _summary = await DashboardService.GetSummaryAsync();
    }
}
````

## Add Web/Components/Pages/DashboardCard.razor

This is a reusable component to prevent duplication!

```` html
<div class="card h-100 shadow-sm">
    <div class="card-body">
        <div class="text-muted small mb-2">
            @Title
        </div>

        <div class="display-6 fw-semibold">
            @Value
        </div>
    </div>
</div>

@code {
    [Parameter]
    public required string Title { get; set; }

    [Parameter]
    public int Value { get; set; }
}
````

## Optional menu Item Addition

```` html
<div class="nav-item px-3">
    <NavLink class="nav-link" href="/dashboard">
        <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Project Dashboard
    </NavLink>
</div>
````

# Discussion & Summary

* Is this secure?
* What could go wrong in the future?
* Thoughts on structure?

