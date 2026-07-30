using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class RoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}

public class CreateRoleDto
{
    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;
}

public class UpdateRoleDto
{
    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;
}
