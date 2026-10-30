using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.EmployeeNo).HasMaxLength(50);
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.RefreshTokenHash).HasMaxLength(255);
        builder.Property(u => u.RefreshTokenExpiryTime);

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Designation)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DesignationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.ReportedSupervisor)
            .WithMany(u => u.SupervisorReports)
            .HasForeignKey(u => u.ReportedSupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.ReportedHod)
            .WithMany(u => u.HodReports)
            .HasForeignKey(u => u.ReportedHodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
