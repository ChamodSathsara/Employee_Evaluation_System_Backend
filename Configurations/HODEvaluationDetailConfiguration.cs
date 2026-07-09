using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class HODEvaluationDetailConfiguration : IEntityTypeConfiguration<HODEvaluationDetail>
{
    public void Configure(EntityTypeBuilder<HODEvaluationDetail> builder)
    {
        builder.ToTable("HODEvaluationDetails");
        builder.HasKey(d => d.DetailId);
        builder.Property(d => d.Marks).HasColumnType("decimal(5,2)");
        builder.Property(d => d.Comments).HasMaxLength(500);

        builder.HasOne(d => d.HODEvaluation)
            .WithMany(e => e.Details)
            .HasForeignKey(d => d.HODEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.KpiAssignmentDetail)
            .WithMany(k => k.HodEvaluationDetails)
            .HasForeignKey(d => d.KPIAssignmentDetailId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
