using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");
        builder.HasKey(a => a.AttachmentId);
        builder.Property(a => a.RelatedTable).HasMaxLength(100).IsRequired();
        builder.Property(a => a.FileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.FilePath).HasMaxLength(500).IsRequired();

        // Polymorphic association - indexed for lookup by owning record, no FK constraint.
        builder.HasIndex(a => new { a.RelatedTable, a.RelatedId });
    }
}
