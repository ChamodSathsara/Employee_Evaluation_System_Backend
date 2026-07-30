using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class EvaluationResultConfiguration : IEntityTypeConfiguration<EvaluationResult>
{
    public void Configure(EntityTypeBuilder<EvaluationResult> builder)
    {
        builder.ToTable("EvaluationResults");
        builder.HasKey(r => r.ResultId);
        builder.Property(r => r.KPIScore).HasColumnType("decimal(5,2)");
        builder.Property(r => r.CompetencyScore).HasColumnType("decimal(5,2)");
        builder.Property(r => r.AchievementScore).HasColumnType("decimal(5,2)");
        builder.Property(r => r.AttendancePenalty).HasColumnType("decimal(5,2)");
        builder.Property(r => r.FinalScore).HasColumnType("decimal(5,2)");
        builder.Property(r => r.Grade).HasMaxLength(20).IsRequired();

        // One evaluation result per HOD evaluation.
        builder.HasOne(r => r.HODEvaluation)
            .WithOne(h => h.Result)
            .HasForeignKey<EvaluationResult>(r => r.HODEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.HODEvaluationId).IsUnique();
    }
}
