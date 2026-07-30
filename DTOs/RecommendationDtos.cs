using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class RecommendationDto
{
    public int RecommendationId { get; set; }
    public int ResultId { get; set; }
    public string RecommendationType { get; set; } = string.Empty;
    public string? RecommendationNote { get; set; }
}

/// <summary>
/// Allowed values for RecommendationType, per the business workflow (Step 14):
/// Promotion, Salary Increment, Training, Extend Probation, No Increment.
/// </summary>
public class CreateRecommendationDto
{
    [Required, MaxLength(100)]
    public string RecommendationType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RecommendationNote { get; set; }
}

public class UpdateRecommendationDto
{
    [Required, MaxLength(100)]
    public string RecommendationType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RecommendationNote { get; set; }
}
