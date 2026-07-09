using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeEvaluationSystem.Configurations;

public class ManagerReviewConfiguration : IEntityTypeConfiguration<ManagerReview>
{
    public void Configure(EntityTypeBuilder<ManagerReview> builder)
    {
        builder.ToTable("ManagerReviews");
        builder.HasKey(m => m.ManagerReviewId);
        builder.Property(m => m.Comments).HasMaxLength(500);
        builder.Property(m => m.Decision).HasMaxLength(50).IsRequired();

        builder.HasOne(m => m.Result)
            .WithMany(r => r.ManagerReviews)
            .HasForeignKey(m => m.ResultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Manager)
            .WithMany()
            .HasForeignKey(m => m.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
