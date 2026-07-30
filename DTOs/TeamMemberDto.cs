namespace EmployeeEvaluationSystem.DTOs;

/// <summary>Lightweight user card shape for team/employee listing screens.</summary>
public class TeamMemberDto
{
    public int UserId { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Whether this member has any KPI assignment (any period) — lets the UI show "Create KPI" vs "View KPIs".</summary>
    public bool HasKpis { get; set; }
}
