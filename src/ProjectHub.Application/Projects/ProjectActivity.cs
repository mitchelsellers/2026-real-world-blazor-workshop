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