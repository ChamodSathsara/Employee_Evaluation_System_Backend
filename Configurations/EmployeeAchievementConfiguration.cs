using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class EmployeeAchievementConfiguration : IEntityTypeConfiguration<EmployeeAchievement>
{
    public void Configure(EntityTypeBuilder<EmployeeAchievement> builder)
    {
        builder.ToTable("EmployeeAchievements");
        builder.HasKey(a => a.AchievementId);
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Marks).HasColumnType("decimal(5,2)");
        builder.Property(a => a.Attachment).HasMaxLength(500);
        builder.Property(a => a.Remarks).HasMaxLength(500);

        builder.HasOne(a => a.HODEvaluation)
            .WithMany(h => h.Achievements)
            .HasForeignKey(a => a.HODEvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.AchievementType)
            .WithMany(t => t.EmployeeAchievements)
            .HasForeignKey(a => a.AchievementTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
