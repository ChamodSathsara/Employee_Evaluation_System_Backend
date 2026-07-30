using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IEmployeeAchievementRepository : IGenericRepository<EmployeeAchievement>
{
    Task<List<EmployeeAchievement>> GetByHodEvaluationAsync(int hodEvaluationId);
    Task<decimal> GetTotalMarksAsync(int hodEvaluationId, int? excludeAchievementId = null);
}
