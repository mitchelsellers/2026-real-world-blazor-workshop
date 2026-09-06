# 001 - Enhancements Steps

## Preventing "Delete" & "Add" commit history

When moving information over, you can use the command-line to move the data elements to the new project.

* Via the command line navigate into your `ProjectHub.Web`'s `Data` folder
* Use the following commands to move the two files, and the migrations to the new location

````
git mv ApplicationDbContext.cs ../../ProjectHub.Data/ApplicationDbContext.cs
md ../../ProjectHub.Data/Models
git mv ApplicationUser.cs ../../ProjectHub.Data/Models/ApplicationUser.cs
````

## Helpful Notes

I've found that it is helpful to drop reminder notes into the DB Context class so you can remember the proper structure for issueing commands.

I simply add a comment to that file with the two commands, my final file looks like the following:

````
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data.Models;

namespace ProjectHub.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    //dotnet ef migrations --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj' add Initial
    //dotnet ef database update --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj'
}
````