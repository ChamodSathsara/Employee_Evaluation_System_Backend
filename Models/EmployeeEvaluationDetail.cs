namespace EmployeeEvaluationSystem.Models;

public class EmployeeEvaluationDetail
{
    public int DetailId { get; set; }
    public int SelfEvaluationId { get; set; }
    public int KPIAssignmentDetailId { get; set; }
    public decimal EmployeeMarks { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public EmployeeEvaluation SelfEvaluation { get; set; } = null!;
    public KPIAssignmentDetail KpiAssignmentDetail { get; set; } = null!;
}
