namespace EmployeeEvaluationSystem.Models;

public class EvaluationPeriod
{
    public int PeriodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public ICollection<KPIAssignment> KpiAssignments { get; set; } = new List<KPIAssignment>();
    public ICollection<AttendanceSummary> AttendanceSummaries { get; set; } = new List<AttendanceSummary>();
}
