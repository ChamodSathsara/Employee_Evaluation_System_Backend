namespace EmployeeEvaluationSystem.Models;

public class EmployeeEvaluation
{
    public int SelfEvaluationId { get; set; }
    public int AssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;

    // Navigation properties
    public KPIAssignment Assignment { get; set; } = null!;
    public User Employee { get; set; } = null!;
    public ICollection<EmployeeEvaluationDetail> Details { get; set; } = new List<EmployeeEvaluationDetail>();
    public ICollection<SupervisorEvaluation> SupervisorEvaluations { get; set; } = new List<SupervisorEvaluation>();
    public ICollection<HODEvaluation> HodEvaluations { get; set; } = new List<HODEvaluation>();
}
