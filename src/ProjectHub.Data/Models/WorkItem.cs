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