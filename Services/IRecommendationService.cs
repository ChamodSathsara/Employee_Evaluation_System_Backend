using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IRecommendationService
{
    Task<List<RecommendationDto>> GetByResultAsync(int callerId, string callerRole, int resultId);
    Task<RecommendationDto> CreateAsync(int hodId, int resultId, CreateRecommendationDto request);
    Task<RecommendationDto> UpdateAsync(int hodId, int recommendationId, UpdateRecommendationDto request);
}
