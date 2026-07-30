using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class EmployeeEvaluationDetailConfiguration : IEntityTypeConfiguration<EmployeeEvaluationDetail>
{
    public void Configure(EntityTypeBuilder<EmployeeEvaluationDetail> builder)
    {
        builder.ToTable("EmployeeEvaluationDetails");
        builder.HasKey(d => d.DetailId);
        builder.Property(d => d.EmployeeMarks).HasColumnType("decimal(5,2)");
        builder.Property(d => d.Comments).HasMaxLength(500);

        builder.HasOne(d => d.SelfEvaluation)
            .WithMany(e => e.Details)
            .HasForeignKey(d => d.SelfEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.KpiAssignmentDetail)
            .WithMany(k => k.EmployeeEvaluationDetails)
            .HasForeignKey(d => d.KPIAssignmentDetailId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
