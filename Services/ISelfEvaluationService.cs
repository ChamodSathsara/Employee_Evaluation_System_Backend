using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface ISelfEvaluationService
{
    Task<EmployeeEvaluationDto> GetMyAsync(int employeeId, int assignmentId);
    Task<List<EmployeeEvaluationDto>> GetMyHistoryAsync(int employeeId);
    Task<EmployeeEvaluationDto?> GetByAssignmentAsync(int callerId, string callerRole, int assignmentId);

    /// <summary>Creates or updates the caller's Draft self-evaluation for the given KPI assignment.</summary>
    Task<EmployeeEvaluationDto> SaveDraftAsync(int employeeId, SaveEmployeeEvaluationDto request);

    /// <summary>Submits the self-evaluation, locking it from further edits and notifying the next reviewer.</summary>
    Task<EmployeeEvaluationDto> SubmitAsync(int employeeId, SaveEmployeeEvaluationDto request);

    Task<EmployeeEvaluationDto> UpdateDraftAsync(int employeeId, int selfEvaluationId, SaveEmployeeEvaluationDto request);
}
