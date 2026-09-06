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