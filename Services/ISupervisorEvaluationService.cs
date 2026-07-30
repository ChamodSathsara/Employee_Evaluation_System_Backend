using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface ISupervisorEvaluationService
{
    Task<List<PendingReviewDto>> GetPendingAsync(int supervisorId);
    Task<SupervisorEvaluationDto> GetByIdAsync(int callerId, string callerRole, int supervisorEvaluationId);
    Task<SupervisorEvaluationDto> CreateAsync(int supervisorId, CreateSupervisorEvaluationDto request);
    Task<SupervisorEvaluationDto> UpdateAsync(int supervisorId, int supervisorEvaluationId, UpdateSupervisorEvaluationDto request);
    Task<SupervisorEvaluationDto> SubmitAsync(int supervisorId, int supervisorEvaluationId);
}
