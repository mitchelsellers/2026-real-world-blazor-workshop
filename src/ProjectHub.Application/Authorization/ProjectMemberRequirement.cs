using Microsoft.AspNetCore.Authorization;

namespace ProjectHub.Application.Authorization;

/// <summary>
/// Used to set a requirement that a user is a member of a project
/// </summary>
public sealed class ProjectMemberRequirement
    : IAuthorizationRequirement;