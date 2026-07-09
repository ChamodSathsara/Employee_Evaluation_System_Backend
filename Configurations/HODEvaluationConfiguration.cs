using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class HODEvaluationConfiguration : IEntityTypeConfiguration<HODEvaluation>
{
    public void Configure(EntityTypeBuilder<HODEvaluation> builder)
    {
        builder.ToTable("HODEvaluations");
        builder.HasKey(e => e.HODEvaluationId);
        builder.Property(e => e.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(e => e.SelfEvaluation)
            .WithMany(s => s.HodEvaluations)
            .HasForeignKey(e => e.SelfEvaluationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.HOD)
            .WithMany()
            .HasForeignKey(e => e.HODId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
