using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("Recommendations");
        builder.HasKey(r => r.RecommendationId);
        builder.Property(r => r.RecommendationType).HasMaxLength(100).IsRequired();
        builder.Property(r => r.RecommendationNote).HasMaxLength(500);

        builder.HasOne(r => r.Result)
            .WithMany(res => res.Recommendations)
            .HasForeignKey(r => r.ResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
