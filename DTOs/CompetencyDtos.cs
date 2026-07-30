using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class CompetencyDto
{
    public int CompetencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCompetencyDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateCompetencyDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class CompetencyEvaluationDto
{
    public int CompetencyEvaluationId { get; set; }
    public int HODEvaluationId { get; set; }
    public int CompetencyId { get; set; }
    public string CompetencyName { get; set; } = string.Empty;
    public int Score { get; set; }
    public string? Remarks { get; set; }
}

public class SubmitCompetencyEvaluationDto
{
    [Required]
    public int CompetencyId { get; set; }

    [Required, Range(1, 5)]
    public int Score { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public class UpdateCompetencyEvaluationDto
{
    [Required, Range(1, 5)]
    public int Score { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}
