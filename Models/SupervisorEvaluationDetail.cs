namespace EmployeeEvaluationSystem.Models;

public class SupervisorEvaluationDetail
{
    public int DetailId { get; set; }
    public int SupervisorEvaluationId { get; set; }
    public int KPIAssignmentDetailId { get; set; }
    public decimal Marks { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public SupervisorEvaluation SupervisorEvaluation { get; set; } = null!;
    public KPIAssignmentDetail KpiAssignmentDetail { get; set; } = null!;
}
