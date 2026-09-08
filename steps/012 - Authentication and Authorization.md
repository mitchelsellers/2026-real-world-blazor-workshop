# Exploring Authentication & Authentication

We already have the ability to login, so lets start simply with doing some changes to autehnticate our applications and show how it works

## Change our Identity User Primary Key

>[!WARNING]
>Only do this if you did not adjust the initial setup steps as outlined in the day of class.  You will need to delete the database to do this safely.

Defaults are for strings, lets go to guids to make it easier for things later.

* Update `ApplicationUser.cs` by adding `<Guid>` after IdentityUser
* Update `ApplicationDbContext.cs` by replacing the inherit code with `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)`
* Add a migration "ChangeToGuidValues"
* Apply the migration

This will allow us better usage going forward

## Update Global Imports

Modify `Components\_Imports.razor` and add the following

```` csharp
@using Microsoft.AspNetCore.Authorization
````

## Secure our endpoints

Add the following code to the top of all views in the "Projects" folder.

```` csharp
@attribute [Authorize]
````

>[!TIP]
>I recommend keeping the attribute close to the top for discoverability.

# Checkpoint - Run the Application

Look at what happens.  NOTE: Default stuff requires that you confirm your email right away, be sure to click the confirm button after registering!

# Next-Steps - Who's the User

We want to create a reusable way to get the current User's id, so we can do stuff with it!

## Create Contract

Create a new file  ProjectHub.Application/Identity/ICurrentUser.cs

```` csharp
namespace ProjectHub.Application.Identity;

public interface ICurrentUser
{    
    Task<Guid?> GetUserIdAsync();
}
````

## Create Implementation

Create another new file CurrentUser.cs

```` csharp
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
````

>[!NOTE]
>You may need to use `Install-Package Microsoft.AspnetCore.Components.Authorization` within the Application project to get the above to compile

## Create Test Implementation

Lets look at this from the homepage, update `Home.razor` to have the following content.

```` razor
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
````



# Future Considerations

* Roles/Attributes
* Adding users to projects
* Conditional Access