using ProjectHub.Data.Models;

namespace ProjectHub.Application.Authorization;

public sealed record ProjectAccessEntry(
    Guid ProjectId,
    ProjectMemberRole Role);