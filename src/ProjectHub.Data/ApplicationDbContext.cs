using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data.Models;

namespace ProjectHub.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    //dotnet ef migrations --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj' add Initial
    //dotnet ef database update --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj'

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    public DbSet<WorkItemComment> WorkItemComments => Set<WorkItemComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Handle Identity relationships and keys
        base.OnModelCreating(modelBuilder);
    }
}
