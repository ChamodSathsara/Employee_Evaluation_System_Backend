using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface ICompetencyEvaluationService
{
    Task<List<CompetencyEvaluationDto>> GetByHodEvaluationAsync(int callerId, string callerRole, int hodEvaluationId);
    Task<CompetencyEvaluationDto> SubmitAsync(int hodId, int hodEvaluationId, SubmitCompetencyEvaluationDto request);
    Task<CompetencyEvaluationDto> UpdateAsync(int hodId, int competencyEvaluationId, UpdateCompetencyEvaluationDto request);
}
