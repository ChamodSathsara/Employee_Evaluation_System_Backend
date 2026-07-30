using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class SupervisorEvaluationDetailDto
{
    public int DetailId { get; set; }
    public int KPIAssignmentDetailId { get; set; }
    public string KPIName { get; set; } = string.Empty;
    public decimal WeightPercentage { get; set; }
    public decimal? EmployeeMarks { get; set; }
    public decimal Marks { get; set; }
    public string? Comments { get; set; }
}

public class SupervisorEvaluationDto
{
    public int SupervisorEvaluationId { get; set; }
    public int SelfEvaluationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int SupervisorId { get; set; }
    public string SupervisorName { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SupervisorEvaluationDetailDto> Details { get; set; } = new();
}

public class SupervisorEvaluationDetailInputDto
{
    [Required]
    public int KPIAssignmentDetailId { get; set; }

    [Required, Range(0, 100)]
    public decimal Marks { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class CreateSupervisorEvaluationDto
{
    [Required]
    public int SelfEvaluationId { get; set; }

    [Required, MinLength(1)]
    public List<SupervisorEvaluationDetailInputDto> Details { get; set; } = new();
}

public class UpdateSupervisorEvaluationDto
{
    [Required, MinLength(1)]
    public List<SupervisorEvaluationDetailInputDto> Details { get; set; } = new();
}

/// <summary>Summary row for the Supervisor's pending-review queue.</summary>
public class PendingReviewDto
{
    public int SelfEvaluationId { get; set; }
    public int AssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
}
