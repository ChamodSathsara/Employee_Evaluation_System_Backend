namespace EmployeeEvaluationSystem.Models;

public class CompetencyEvaluation
{
    public int CompetencyEvaluationId { get; set; }
    public int HODEvaluationId { get; set; }
    public int CompetencyId { get; set; }
    public int Score { get; set; }
    public string? Remarks { get; set; }

    // Navigation properties
    public HODEvaluation HODEvaluation { get; set; } = null!;
    public Competency Competency { get; set; } = null!;
}
