using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IEvaluationPeriodService
{
    Task<List<EvaluationPeriodDto>> GetAllAsync();
    Task<EvaluationPeriodDto> GetByIdAsync(int periodId);
    Task<EvaluationPeriodDto> CreateAsync(CreateEvaluationPeriodDto request);
    Task<EvaluationPeriodDto> UpdateAsync(int periodId, UpdateEvaluationPeriodDto request);

    /// <summary>Activates the given period and deactivates every other period (only one period is Active at a time).</summary>
    Task<EvaluationPeriodDto> ActivateAsync(int periodId);

    Task DeleteAsync(int periodId);
}
