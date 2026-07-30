namespace EmployeeEvaluationSystem.Models;

public class KPIAssignment
{
    public int AssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public int CreatedBy { get; set; }
    public int PeriodId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    // Navigation properties
    public User Employee { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public EvaluationPeriod Period { get; set; } = null!;
    public ICollection<KPIAssignmentDetail> Details { get; set; } = new List<KPIAssignmentDetail>();
    public ICollection<EmployeeEvaluation> Evaluations { get; set; } = new List<EmployeeEvaluation>();
}
