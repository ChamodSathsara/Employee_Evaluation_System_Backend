using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class EmployeeEvaluationConfiguration : IEntityTypeConfiguration<EmployeeEvaluation>
{
    public void Configure(EntityTypeBuilder<EmployeeEvaluation> builder)
    {
        builder.ToTable("EmployeeEvaluations");
        builder.HasKey(e => e.SelfEvaluationId);
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(e => e.Assignment)
            .WithMany(a => a.Evaluations)
            .HasForeignKey(e => e.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
