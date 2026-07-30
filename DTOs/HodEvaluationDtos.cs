using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class HodEvaluationDetailDto
{
    public int DetailId { get; set; }
    public int KPIAssignmentDetailId { get; set; }
    public string KPIName { get; set; } = string.Empty;
    public decimal WeightPercentage { get; set; }
    public decimal? EmployeeMarks { get; set; }
    public decimal? SupervisorMarks { get; set; }
    public decimal Marks { get; set; }
    public string? Comments { get; set; }
}

public class HodEvaluationDto
{
    public int HODEvaluationId { get; set; }
    public int SelfEvaluationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int HODId { get; set; }
    public string HODName { get; set; } = string.Empty;
    public DateTime? SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<HodEvaluationDetailDto> Details { get; set; } = new();
}

public class HodEvaluationDetailInputDto
{
    [Required]
    public int KPIAssignmentDetailId { get; set; }

    [Required, Range(0, 100)]
    public decimal Marks { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class CreateHodEvaluationDto
{
    [Required]
    public int SelfEvaluationId { get; set; }

    [Required, MinLength(1)]
    public List<HodEvaluationDetailInputDto> Details { get; set; } = new();
}

public class UpdateHodEvaluationDto
{
    [Required, MinLength(1)]
    public List<HodEvaluationDetailInputDto> Details { get; set; } = new();
}
