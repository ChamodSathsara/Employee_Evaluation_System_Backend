namespace EmployeeEvaluationSystem.Models;

public class ManagerReview
{
    public int ManagerReviewId { get; set; }
    public int ResultId { get; set; }
    public int ManagerId { get; set; }
    public string? Comments { get; set; }
    public string Decision { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }

    // Navigation properties
    public EvaluationResult Result { get; set; } = null!;
    public User Manager { get; set; } = null!;
}
