using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IKpiAssignmentService
{
    /// <summary>
    /// Creates a new Draft KPI assignment. Enforces: CEO->Manager, Manager->HOD,
    /// HOD->Supervisor|Employee, Supervisor->Employee, the target must directly
    /// report to the creator, and the KPI line-item weights must sum to exactly 100%.
    /// </summary>
    Task<KpiAssignmentDto> CreateAsync(int callerId, CreateKpiAssignmentDto request);

    /// <summary>Replaces the line items of a Draft KPI assignment. Only the original creator may edit, and only while Status = Draft.</summary>
    Task<KpiAssignmentDto> UpdateAsync(int callerId, int assignmentId, UpdateKpiAssignmentDto request);

    /// <summary>Deletes a Draft KPI assignment. Only the original creator may delete, and only while Status = Draft with no evaluations started.</summary>
    Task DeleteAsync(int callerId, int assignmentId);

    /// <summary>Transitions Draft -> Published, making the KPI set visible to the employee and locking it from further edits.</summary>
    Task<KpiAssignmentDto> PublishAsync(int callerId, int assignmentId);

    /// <summary>Transitions Published -> Draft, reopening the KPI set for correction by its creator.</summary>
    Task<KpiAssignmentDto> RevertToDraftAsync(int callerId, int assignmentId);

    Task<List<KpiAssignmentDto>> GetMyKpisAsync(int callerId);

    Task<KpiAssignmentDto> GetByIdAsync(int callerId, string callerRole, int assignmentId);

    Task<List<KpiAssignmentDto>> GetByEmployeeAsync(int callerId, string callerRole, int employeeId);

    Task<List<KpiAssignmentDto>> GetByPeriodAsync(int callerId, string callerRole, int periodId);

    /// <summary>
    /// Returns every KPI assignment (all periods) belonging to the caller's direct reports.
    /// Used by CEO/Manager/HOD/Supervisor "Employees" / team screens to show each team
    /// member's KPI status without a per-employee round trip.
    /// </summary>
    Task<List<KpiAssignmentDto>> GetMyTeamKpisAsync(int callerId);
}