using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class SupervisorEvaluationDetailConfiguration : IEntityTypeConfiguration<SupervisorEvaluationDetail>
{
    public void Configure(EntityTypeBuilder<SupervisorEvaluationDetail> builder)
    {
        builder.ToTable("SupervisorEvaluationDetails");
        builder.HasKey(d => d.DetailId);
        builder.Property(d => d.Marks).HasColumnType("decimal(5,2)");
        builder.Property(d => d.Comments).HasMaxLength(500);

        builder.HasOne(d => d.SupervisorEvaluation)
            .WithMany(e => e.Details)
            .HasForeignKey(d => d.SupervisorEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.KpiAssignmentDetail)
            .WithMany(k => k.SupervisorEvaluationDetails)
            .HasForeignKey(d => d.KPIAssignmentDetailId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
