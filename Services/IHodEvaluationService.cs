using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IHodEvaluationService
{
    Task<List<PendingReviewDto>> GetPendingAsync(int hodId);
    Task<HodEvaluationDto> GetByIdAsync(int callerId, string callerRole, int hodEvaluationId);
    Task<HodEvaluationDto> CreateAsync(int hodId, CreateHodEvaluationDto request);
    Task<HodEvaluationDto> UpdateAsync(int hodId, int hodEvaluationId, UpdateHodEvaluationDto request);
    Task<HodEvaluationDto> SubmitAsync(int hodId, int hodEvaluationId);
}
