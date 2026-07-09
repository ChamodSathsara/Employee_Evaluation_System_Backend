namespace EmployeeEvaluationSystem.Models;

public class SupervisorEvaluation
{
    public int SupervisorEvaluationId { get; set; }
    public int SelfEvaluationId { get; set; }
    public int SupervisorId { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;

    // Navigation properties
    public EmployeeEvaluation SelfEvaluation { get; set; } = null!;
    public User Supervisor { get; set; } = null!;
    public ICollection<SupervisorEvaluationDetail> Details { get; set; } = new List<SupervisorEvaluationDetail>();
}
