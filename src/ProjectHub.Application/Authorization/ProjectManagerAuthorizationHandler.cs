using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data;
using ProjectHub.Data.Models;
using System.Security.Claims;

namespace ProjectHub.Application.Authorization;

[RegisterScoped<IAuthorizationHandler>]
internal sealed class ProjectManagerAuthorizationHandler(
    IDbContextFactory<ApplicationDbContext> contextFactory)
    : AuthorizationHandler<
        ProjectManagerRequirement,
        ProjectAuthorizationResource>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectManagerRequirement requirement,
        ProjectAuthorizationResource resource)
    {
        var userIdValue =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        using var db = await contextFactory.CreateDbContextAsync();

        var allowed =
            await db.ProjectMembers
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ProjectId == resource.ProjectId &&
                    x.Role == ProjectMemberRole.ProjectManager &&
                    x.UserId == userId);

        if (allowed)
        {
            context.Succeed(requirement);
        }
    }
}