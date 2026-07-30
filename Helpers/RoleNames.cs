namespace EmployeeEvaluationSystem.Helpers;

/// <summary>
/// Canonical role name constants, matching the RoleName values seeded in the Roles table.
/// Used for [Authorize(Roles = ...)] attributes and hierarchy/business-rule checks so the
/// role names are never hard-coded as magic strings in more than one place.
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string CEO = "CEO";
    public const string Manager = "Manager";
    public const string HOD = "HOD";
    public const string Supervisor = "Supervisor";
    public const string Employee = "Employee";
}
