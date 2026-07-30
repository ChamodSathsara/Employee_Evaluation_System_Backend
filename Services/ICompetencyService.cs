using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface ICompetencyService
{
    Task<List<CompetencyDto>> GetAllAsync();
    Task<CompetencyDto> CreateAsync(CreateCompetencyDto request);
    Task<CompetencyDto> UpdateAsync(int competencyId, UpdateCompetencyDto request);
    Task DeleteAsync(int competencyId);
}
