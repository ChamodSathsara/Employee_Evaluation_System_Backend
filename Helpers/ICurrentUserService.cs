namespace EmployeeEvaluationSystem.Helpers;

public interface ICurrentUserService
{
    /// <summary>UserId of the currently authenticated caller, or null if unauthenticated.</summary>
    int? UserId { get; }

    /// <summary>Role name of the currently authenticated caller (e.g. "Admin", "CEO", "HOD"), or null if unauthenticated.</summary>
    string? RoleName { get; }

    /// <summary>DepartmentId of the currently authenticated caller, or null if unauthenticated.</summary>
    int? DepartmentId { get; }

    bool IsInRole(string roleName);
}
