using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
}

public class CreateDepartmentDto
{
    [Required, MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}

public class UpdateDepartmentDto
{
    [Required, MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}

/// <summary>
/// Department card summary for the Manager/CEO "Departments" screen: employee count,
/// HOD/Supervisor name(s), and completion percentages for grading and attendance for a
/// given evaluation period.
/// </summary>
public class DepartmentProgressDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public List<string> HodNames { get; set; } = new();
    public List<string> SupervisorNames { get; set; } = new();

    /// <summary>Percentage of this department's employees whose evaluation has a Final Score/Grade for the given period.</summary>
    public decimal GradeCompletionPercentage { get; set; }

    /// <summary>Percentage of this department's employees who have an AttendanceSummary recorded for the given period.</summary>
    public decimal AttendanceCompletionPercentage { get; set; }
}
