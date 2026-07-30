using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IRecommendationRepository : IGenericRepository<Recommendation>
{
    Task<List<Recommendation>> GetByResultAsync(int resultId);
}
