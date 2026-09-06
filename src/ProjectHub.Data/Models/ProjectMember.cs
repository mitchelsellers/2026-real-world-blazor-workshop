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