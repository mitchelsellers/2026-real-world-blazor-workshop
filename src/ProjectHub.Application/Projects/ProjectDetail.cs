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