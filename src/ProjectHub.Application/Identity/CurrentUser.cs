using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ProjectHub.Application.Identity;

[RegisterScoped]
public sealed class CurrentUser(AuthenticationStateProvider authenticationStateProvider)
    : ICurrentUser
{
    public async Task<Guid?> GetUserIdAsync()
    {
        var authState = await authenticationStateProvider
                .GetAuthenticationStateAsync();

        var userIdValue = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }
}