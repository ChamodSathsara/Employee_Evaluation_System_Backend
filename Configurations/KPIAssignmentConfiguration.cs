using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class KPIAssignmentConfiguration : IEntityTypeConfiguration<KPIAssignment>
{
    public void Configure(EntityTypeBuilder<KPIAssignment> builder)
    {
        builder.ToTable("KPIAssignments");
        builder.HasKey(a => a.AssignmentId);
        builder.Property(a => a.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Period)
            .WithMany(p => p.KpiAssignments)
            .HasForeignKey(a => a.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
