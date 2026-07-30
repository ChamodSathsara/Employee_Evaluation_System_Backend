namespace EmployeeEvaluationSystem.Models;

public class AttendanceSummary
{
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public int PeriodId { get; set; }
    public decimal AnnualLeave { get; set; }
    public decimal CasualLeave { get; set; }
    public decimal MedicalLeave { get; set; }
    public decimal NoPayLeave { get; set; }
    public int LateAttendance { get; set; }
    public decimal PenaltyMarks { get; set; }

    // Navigation properties
    public User Employee { get; set; } = null!;
    public EvaluationPeriod Period { get; set; } = null!;
}
