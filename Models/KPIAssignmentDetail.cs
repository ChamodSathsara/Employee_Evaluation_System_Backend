namespace EmployeeEvaluationSystem.Models;

public class KPIAssignmentDetail
{
    public int DetailId { get; set; }
    public int AssignmentId { get; set; }
    public string KPIName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal WeightPercentage { get; set; }

    // Navigation properties
    public KPIAssignment Assignment { get; set; } = null!;
    public ICollection<EmployeeEvaluationDetail> EmployeeEvaluationDetails { get; set; } = new List<EmployeeEvaluationDetail>();
    public ICollection<SupervisorEvaluationDetail> SupervisorEvaluationDetails { get; set; } = new List<SupervisorEvaluationDetail>();
    public ICollection<HODEvaluationDetail> HodEvaluationDetails { get; set; } = new List<HODEvaluationDetail>();
}
