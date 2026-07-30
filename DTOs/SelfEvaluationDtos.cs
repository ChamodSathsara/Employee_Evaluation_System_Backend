using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class EmployeeEvaluationDetailDto
{
    public int DetailId { get; set; }
    public int KPIAssignmentDetailId { get; set; }
    public string KPIName { get; set; } = string.Empty;
    public decimal WeightPercentage { get; set; }
    public decimal EmployeeMarks { get; set; }
    public string? Comments { get; set; }
}

public class EmployeeEvaluationDto
{
    public int SelfEvaluationId { get; set; }
    public int AssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<EmployeeEvaluationDetailDto> Details { get; set; } = new();
}

public class SaveEmployeeEvaluationDetailDto
{
    [Required]
    public int KPIAssignmentDetailId { get; set; }

    [Required, Range(0, 100)]
    public decimal EmployeeMarks { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class SaveEmployeeEvaluationDto
{
    [Required]
    public int AssignmentId { get; set; }

    [Required, MinLength(1)]
    public List<SaveEmployeeEvaluationDetailDto> Details { get; set; } = new();
}
