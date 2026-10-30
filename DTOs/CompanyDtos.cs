using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class CompanyDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}

public class CreateCompanyDto
{
    [Required, MaxLength(150)]
    public string CompanyName { get; set; } = string.Empty;
}
