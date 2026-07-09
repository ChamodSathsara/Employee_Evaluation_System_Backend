using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class KpiAssignmentDetailDto
{
    public int DetailId { get; set; }
    public string KPIName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal WeightPercentage { get; set; }
}

public class KpiAssignmentDto
{
    public int AssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalWeight { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<KpiAssignmentDetailDto> Details { get; set; } = new();
}

public class CreateKpiAssignmentDetailDto
{
    [Required, MaxLength(200)]
    public string KPIName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, Range(0.01, 100)]
    public decimal WeightPercentage { get; set; }
}

public class CreateKpiAssignmentDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int PeriodId { get; set; }

    [Required, MinLength(1, ErrorMessage = "At least one KPI line item is required.")]
    public List<CreateKpiAssignmentDetailDto> Details { get; set; } = new();
}

public class UpdateKpiAssignmentDto
{
    [Required, MinLength(1, ErrorMessage = "At least one KPI line item is required.")]
    public List<CreateKpiAssignmentDetailDto> Details { get; set; } = new();
}
