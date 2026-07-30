using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class AttendanceSummaryDto
{
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int PeriodId { get; set; }
    public decimal AnnualLeave { get; set; }
    public decimal CasualLeave { get; set; }
    public decimal MedicalLeave { get; set; }
    public decimal NoPayLeave { get; set; }
    public int LateAttendance { get; set; }
    public decimal PenaltyMarks { get; set; }
}

public class UpsertAttendanceSummaryDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int PeriodId { get; set; }

    [Range(0, 365)]
    public decimal AnnualLeave { get; set; }

    [Range(0, 365)]
    public decimal CasualLeave { get; set; }

    [Range(0, 365)]
    public decimal MedicalLeave { get; set; }

    [Range(0, 365)]
    public decimal NoPayLeave { get; set; }

    [Range(0, 365)]
    public int LateAttendance { get; set; }
}

/// <summary>One row of an attendance import batch (e.g. parsed from an uploaded CSV/Excel file).</summary>
public class AttendanceImportRowDto
{
    [Required]
    public int EmployeeId { get; set; }

    public decimal AnnualLeave { get; set; }
    public decimal CasualLeave { get; set; }
    public decimal MedicalLeave { get; set; }
    public decimal NoPayLeave { get; set; }
    public int LateAttendance { get; set; }
}

public class ImportAttendanceDto
{
    [Required]
    public int PeriodId { get; set; }

    [Required, MinLength(1)]
    public List<AttendanceImportRowDto> Rows { get; set; } = new();
}
