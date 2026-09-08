# Work Item Management

Lets start getting to some CRUD operations

* Create a new folder `WorkItems` within the .Application project

## Create `CreateWorkItemRequest`

This will be our initial model class, we are using a true class as we want to work with DataAnnotations and more.

```` csharp
using ProjectHub.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Application.WorkItems;

public sealed class CreateWorkItemRequest
{
    [Required]
    [Display(Name = "Project")]
    public Guid ProjectId { get; set; }
    [Required]
    [StringLength(300)]
    [Display(Name = "Title")]
    public string Title { get; set; } = null!;
    [StringLength(4000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    [Display(Name = "Priority")]
    public WorkItemPriority Priority { get; set; }
    [Display(Name = "Due Date")]
    public DateTime? DueDateUtc { get; set; }
}
````

## Create `IWorkItemService`

Just like the other one add to the `WorkItems` Folder

```` csharp
namespace ProjectHub.Application.WorkItems;

public interface IWorkItemService
{
    Task<Guid> CreateAsync(CreateWorkItemRequest request, CancellationToken cancellationToken = default);
}
````

## Create `WorkItemService`

Again, new file in the `WorkItems` folder

```` csharp
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Data.Models;

namespace ProjectHub.Application.WorkItems;

[RegisterScoped]
internal sealed class WorkItemService(IDbContextFactory<ApplicationDbContext> contextFactory)
    : IWorkItemService
{
    public async Task<Guid> CreateAsync(CreateWorkItemRequest request, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        //Validate that the project exists before creating a work item for it
        var projectExists = await db.Projects.AnyAsync(x => x.ProjectId == request.ProjectId, cancellationToken);

        if (!projectExists)
        {
            throw new InvalidOperationException("Project was not found.");
        }

        var item = new WorkItem
        {
            ProjectId = request.ProjectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            DueDateUtc = request.DueDateUtc,
            Status = WorkItemStatus.New
        };

        db.WorkItems.Add(item);

        await db.SaveChangesAsync(cancellationToken);

        return item.WorkItemId;
    }
}
````

## Create the Add Form

This will be our addition form, add a folder under `Projects` within the web, and then create a new component `New.razor` and provide the following content.

```` razor
@page "/projects/{ProjectId:guid}/work-items/new"

@rendermode InteractiveServer

@using System.ComponentModel.DataAnnotations
@using ProjectHub.Application.WorkItems
@using ProjectHub.Data.Models

@inject IWorkItemService WorkItemService
@inject NavigationManager NavigationManager

<PageTitle>New Work Item</PageTitle>

<div class="container py-4">
	<div class="row">
		<div class="col-lg-8">

			<div class="mb-4">
				<a href="@($"/projects/{ProjectId}")"
				   class="text-decoration-none">
					&larr; Back to Project
				</a>
			</div>

			<h1>New Work Item</h1>

			<p class="text-muted">
				Add a new work item to this project.
			</p>

			@if (!string.IsNullOrWhiteSpace(_errorMessage))
			{
				<div class="alert alert-danger" role="alert">
					@_errorMessage
				</div>
			}

			<EditForm Model="_model" OnValidSubmit="HandleValidSubmit">
				<DataAnnotationsValidator />

				<div class="mb-3">
					<label for="title" class="form-label">
						Title
					</label>

					<InputText id="title"
							   class="form-control"
							   @bind-Value="_model.Title" />

					<ValidationMessage For="@(() => _model.Title)" />
				</div>

				<div class="mb-3">
					<label for="description" class="form-label">
						Description
					</label>

					<InputTextArea id="description"
								   class="form-control"
								   rows="5"
								   @bind-Value="_model.Description" />

					<ValidationMessage For="@(() => _model.Description)" />
				</div>

				<div class="row">
					<div class="col-md-6 mb-3">
						<label for="priority" class="form-label">
							Priority
						</label>

						<InputSelect id="priority"
									 class="form-select"
									 @bind-Value="_model.Priority">
							@foreach (var priority in Enum.GetValues<WorkItemPriority>())
							{
								<option value="@priority">
									@priority
								</option>
							}
						</InputSelect>
					</div>

					<div class="col-md-6 mb-3">
						<label for="dueDate" class="form-label">
							Due Date
						</label>

						<InputDate id="dueDate"
								   class="form-control"
								   @bind-Value="_model.DueDateUtc" />
					</div>
				</div>

				<div class="d-flex gap-2">
					<button type="submit"
							class="btn btn-primary"
							disabled="@_isSaving">
						@if (_isSaving)
						{
							<span class="spinner-border spinner-border-sm me-2"
								  aria-hidden="true">
							</span>

							<span>Saving...</span>
						}
						else
						{
							<span>Create Work Item</span>
						}
					</button>

					<a href="@($"/projects/{ProjectId}")"
					   class="btn btn-outline-secondary">
						Cancel
					</a>
				</div>

			</EditForm>
		</div>
	</div>
</div>

@code {
	[Parameter]
	public Guid ProjectId { get; set; }

	private readonly CreateWorkItemRequest _model = new();

	private bool _isSaving;

	private string? _errorMessage;

	private async Task HandleValidSubmit()
	{
		_errorMessage = null;
		_isSaving = true;

		try
		{
			//Set our project Id
			_model.ProjectId = ProjectId;
			await WorkItemService.CreateAsync(_model);

			NavigationManager.NavigateTo($"/projects/{ProjectId}");
		}
		catch (InvalidOperationException ex)
		{
			_errorMessage = ex.Message;
		}
		finally
		{
			_isSaving = false;
		}
	}
}
````

## Update Detail Page to Have Link

Replace the current heading in the Details page with the following content

```` html
<div class="d-flex justify-content-between align-items-center mb-3">
    <h2 class="mb-0">Work Items</h2>

    <a href="@($"/projects/{ProjectId}/work-items/new")"
       class="btn btn-primary">
        Add Work Item
    </a>
</div>
````

## Discussion Items

* Is Data Annotations Good Enough?
* Do we need a further abstraction around the creation
* User Interactions
