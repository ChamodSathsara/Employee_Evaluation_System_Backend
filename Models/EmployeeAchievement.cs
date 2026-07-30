namespace EmployeeEvaluationSystem.Models;

public class EmployeeAchievement
{
    public int AchievementId { get; set; }
    public int HODEvaluationId { get; set; }
    public int AchievementTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Marks { get; set; }
    public string? Attachment { get; set; }
    public string? Remarks { get; set; }

    // Navigation properties
    public HODEvaluation HODEvaluation { get; set; } = null!;
    public AchievementType AchievementType { get; set; } = null!;
}
