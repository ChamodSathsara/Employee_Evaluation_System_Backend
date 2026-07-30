namespace EmployeeEvaluationSystem.Models;

public class HODEvaluation
{
    public int HODEvaluationId { get; set; }
    public int SelfEvaluationId { get; set; }
    public int HODId { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;

    // Navigation properties
    public EmployeeEvaluation SelfEvaluation { get; set; } = null!;
    public User HOD { get; set; } = null!;
    public ICollection<HODEvaluationDetail> Details { get; set; } = new List<HODEvaluationDetail>();
    public ICollection<CompetencyEvaluation> CompetencyEvaluations { get; set; } = new List<CompetencyEvaluation>();
    public ICollection<EmployeeAchievement> Achievements { get; set; } = new List<EmployeeAchievement>();
    public EvaluationResult? Result { get; set; }
}
