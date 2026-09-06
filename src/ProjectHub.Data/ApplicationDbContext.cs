using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data.Models;

namespace ProjectHub.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    //dotnet ef migrations --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj' add Initial
    //dotnet ef database update --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj'
}
