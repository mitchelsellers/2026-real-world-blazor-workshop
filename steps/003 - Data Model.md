# Creation of the Data Model

The goal of this step is to setup the data for our project!  We will discuss a few pros/cons of various structures, including the compromise/behaviors that we are using within this solution.

## EF Core Modeling Options

EF Core can use Attributes and/or fluent syntax for configuration. 

For the purposes of keeping the project clean we will be using the Fluent Syntax here, with the `IEntityTypeConfiguration<>` behavior. 

## Individual Data Files

Each of the following will be new files added to the `ProjectHub.Data\Models` folder.

>[!Note]
>When adding these files you may see build errors until you have added ALL files

### Project.cs

This represents a single project within our solution.

```` Project.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectHub.Data.Models;

public sealed class Project
{
    public Guid ProjectId { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;

    public ICollection<WorkItem> WorkItems { get; set; } = [];

    public ICollection<ProjectMember> Members { get; set; } = [];
}

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(x => x.ProjectId);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        //We know we are going to query on this directly
        builder.HasIndex(x => x.Status);

        // Explicitly configure the relationships to ensure cascading delete behavior, or restriction if we want
        builder.HasMany(x => x.Members)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.WorkItems)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
````

### ProjectMember.cs

This holds an association or permissions for a user into a particular project.

```` csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectHub.Data.Models;

public sealed class ProjectMember
{
    public Guid ProjectMemberId { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Member;

    public DateTime AddedOnUtc { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
}

internal sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(x => x.ProjectMemberId);

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        // Make sure that we cannot have the same user twice
        builder.HasIndex(x => new
        {
            x.ProjectId,
            x.UserId
        })
        .IsUnique();

        // TODO: Future Consideration for linking to the EF Tables
    }
}
````
### ProjectMemberRole.cs

The roles available for project members.

```` csharp
namespace ProjectHub.Data.Models;

public enum ProjectMemberRole
{
    Member = 1,
    ProjectManager = 2
}
````

### ProjectStatus.cs

Create a new file for storing the enum of possible status values and replace the contents of the newly created file with the below

```` csharp
using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Data.Models;

public enum ProjectStatus
{
    Active = 1,
    [Display(Name = "On Hold")]
    OnHold = 2,
    Completed = 3
}
````

This represents our project status of Active, On Hold, or Completed

### WorkItem.cs

This is an individual work item within the project

```` csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectHub.Data.Models;

public sealed class WorkItem
{
    public Guid WorkItemId { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public WorkItemStatus Status { get; set; } = WorkItemStatus.New;

    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Normal;

    public Guid? AssignedToUserId { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;

    public ICollection<WorkItemComment> Comments { get; set; } = [];
}

internal sealed class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.HasKey(x => x.WorkItemId);

        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.AssignedToUserId)
            .HasMaxLength(450);

        builder.HasIndex(x => x.ProjectId);

        builder.HasIndex(x => new
        {
            x.ProjectId,
            x.Status
        });

        builder.HasIndex(x => x.AssignedToUserId);

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.WorkItem)
            .HasForeignKey(x => x.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
````

### WorkItemComment.cs

This is a comment on a particular work item

```` csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProjectHub.Data.Models;

public sealed class WorkItemComment
{
    public Guid WorkItemCommentId { get; set; } = Guid.NewGuid();

    public Guid WorkItemId { get; set; }

    public required Guid UserId { get; set; }

    public required string Comment { get; set; }

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public WorkItem WorkItem { get; set; } = null!;
}

internal sealed class WorkItemCommentConfiguration : IEntityTypeConfiguration<WorkItemComment>
{
    public void Configure(EntityTypeBuilder<WorkItemComment> builder)
    {
        builder.HasKey(x => x.WorkItemCommentId);

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.WorkItemId,
            x.CreatedOnUtc
        });
    }
}
````

### WorkItemPriority.cs

For the priority of the item

```` csharp
namespace ProjectHub.Data.Models;

public enum WorkItemPriority
{
    Low = 1,
    Normal = 2,
    High = 3
}
````

### WorkItemStatus.cs

For the overall status

```` csharp
namespace ProjectHub.Data.Models;

public enum WorkItemStatus
{
    New = 1,
    InProgress = 2,
    Blocked = 3,
    Completed = 4
}
````

## Updating DB Context

Now that we have our items, we need to add them to the DB Context.  Given that we didn't really have much there already, replace the full `AppliationDbContext.cs` file with the following.

```` csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectHub.Data.Models;

namespace ProjectHub.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
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
````

## Creating the Migration & Update DB

Lets go ahead and create the Migration

````
dotnet ef migrations --startup-project '../ProjectHub.Web/ProjectHub.Web.csproj' add ProjectTables
````

>[!TIP]
>If you would like you can use the update command as commented in the source.  Additionally if you would like to script to see what it might do you can use `dotnet ef migrations script -o ./deploy-script.sql --startup-project "../ProjectHub.Web/ProductHub.Web.csproj" -i --no-build` We will discuss this a bit.
