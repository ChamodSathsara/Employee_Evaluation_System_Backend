using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class SupervisorEvaluationConfiguration : IEntityTypeConfiguration<SupervisorEvaluation>
{
    public void Configure(EntityTypeBuilder<SupervisorEvaluation> builder)
    {
        builder.ToTable("SupervisorEvaluations");
        builder.HasKey(e => e.SupervisorEvaluationId);
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(e => e.SelfEvaluation)
            .WithMany(s => s.SupervisorEvaluations)
            .HasForeignKey(e => e.SelfEvaluationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Supervisor)
            .WithMany()
            .HasForeignKey(e => e.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
