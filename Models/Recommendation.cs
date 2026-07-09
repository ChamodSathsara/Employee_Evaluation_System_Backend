namespace EmployeeEvaluationSystem.Models;

public class Recommendation
{
    public int RecommendationId { get; set; }
    public int ResultId { get; set; }
    public string RecommendationType { get; set; } = string.Empty;
    public string? RecommendationNote { get; set; }

    // Navigation properties
    public EvaluationResult Result { get; set; } = null!;
}
