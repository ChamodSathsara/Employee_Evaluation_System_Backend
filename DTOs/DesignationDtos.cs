using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class DesignationDto
{
    public int DesignationId { get; set; }
    public string DesignationName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}

public class CreateDesignationDto
{
    [Required, MaxLength(100)]
    public string DesignationName { get; set; } = string.Empty;
}

public class UpdateDesignationDto
{
    [Required, MaxLength(100)]
    public string DesignationName { get; set; } = string.Empty;
}
