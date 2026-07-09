using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class AttendanceSummaryConfiguration : IEntityTypeConfiguration<AttendanceSummary>
{
    public void Configure(EntityTypeBuilder<AttendanceSummary> builder)
    {
        builder.ToTable("AttendanceSummaries");
        builder.HasKey(a => a.AttendanceId);
        builder.Property(a => a.AnnualLeave).HasColumnType("decimal(5,2)");
        builder.Property(a => a.CasualLeave).HasColumnType("decimal(5,2)");
        builder.Property(a => a.MedicalLeave).HasColumnType("decimal(5,2)");
        builder.Property(a => a.NoPayLeave).HasColumnType("decimal(5,2)");
        builder.Property(a => a.PenaltyMarks).HasColumnType("decimal(5,2)");

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Period)
            .WithMany(p => p.AttendanceSummaries)
            .HasForeignKey(a => a.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EmployeeId, a.PeriodId }).IsUnique();
    }
}
