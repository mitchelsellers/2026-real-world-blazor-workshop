using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProjectHub.Application.Authorization;

[RegisterScoped<IAuthorizationHandler>]
internal sealed class ProjectMemberAuthorizationHandler(
    IProjectAccessService projectAccessService)
    : AuthorizationHandler<
        ProjectMemberRequirement,
        ProjectAuthorizationResource>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectMemberRequirement requirement,
        ProjectAuthorizationResource resource)
    {
        var userIdValue =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return;
        }

        if (await projectAccessService.IsMemberAsync(
            userId,
            resource.ProjectId))
        {
            context.Succeed(requirement);
        }
    }
}