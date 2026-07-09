using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class CompetencyEvaluationConfiguration : IEntityTypeConfiguration<CompetencyEvaluation>
{
    public void Configure(EntityTypeBuilder<CompetencyEvaluation> builder)
    {
        builder.ToTable("CompetencyEvaluations");
        builder.HasKey(c => c.CompetencyEvaluationId);
        builder.Property(c => c.Remarks).HasMaxLength(500);

        builder.HasOne(c => c.HODEvaluation)
            .WithMany(h => h.CompetencyEvaluations)
            .HasForeignKey(c => c.HODEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Competency)
            .WithMany(comp => comp.CompetencyEvaluations)
            .HasForeignKey(c => c.CompetencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
