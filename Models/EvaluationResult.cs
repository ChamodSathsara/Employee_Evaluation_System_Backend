namespace EmployeeEvaluationSystem.Models;

public class EvaluationResult
{
    public int ResultId { get; set; }
    public int HODEvaluationId { get; set; }
    public decimal KPIScore { get; set; }
    public decimal CompetencyScore { get; set; }
    public decimal AchievementScore { get; set; }
    public decimal AttendancePenalty { get; set; }
    public decimal FinalScore { get; set; }
    public string Grade { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }

    // Navigation properties
    public HODEvaluation HODEvaluation { get; set; } = null!;
    public ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    public ICollection<ManagerReview> ManagerReviews { get; set; } = new List<ManagerReview>();
}
