using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class AchievementTypeConfiguration : IEntityTypeConfiguration<AchievementType>
{
    public void Configure(EntityTypeBuilder<AchievementType> builder)
    {
        builder.ToTable("AchievementTypes");
        builder.HasKey(a => a.AchievementTypeId);
        builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
    }
}
