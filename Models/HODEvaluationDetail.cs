namespace EmployeeEvaluationSystem.Models;

public class HODEvaluationDetail
{
    public int DetailId { get; set; }
    public int HODEvaluationId { get; set; }
    public int KPIAssignmentDetailId { get; set; }
    public decimal Marks { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public HODEvaluation HODEvaluation { get; set; } = null!;
    public KPIAssignmentDetail KpiAssignmentDetail { get; set; } = null!;
}
