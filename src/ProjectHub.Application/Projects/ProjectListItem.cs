using ProjectHub.Data.Models;

namespace ProjectHub.Application.Projects;

public sealed record ProjectListItem(
    Guid ProjectId,
    string Name,
    ProjectStatus Status,
    int OpenWorkItemCount);