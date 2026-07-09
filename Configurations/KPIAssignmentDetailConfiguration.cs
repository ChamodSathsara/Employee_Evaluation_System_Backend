using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class KPIAssignmentDetailConfiguration : IEntityTypeConfiguration<KPIAssignmentDetail>
{
    public void Configure(EntityTypeBuilder<KPIAssignmentDetail> builder)
    {
        builder.ToTable("KPIAssignmentDetails");
        builder.HasKey(d => d.DetailId);
        builder.Property(d => d.KPIName).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.WeightPercentage).HasColumnType("decimal(5,2)");

        builder.HasOne(d => d.Assignment)
            .WithMany(a => a.Details)
            .HasForeignKey(d => d.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
