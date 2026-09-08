namespace ProjectHub.Application.WorkItems;

public interface IWorkItemService
{
    Task<Guid> CreateAsync(CreateWorkItemRequest request, CancellationToken cancellationToken = default);
}