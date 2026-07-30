using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IAchievementTypeRepository : IGenericRepository<AchievementType>
{
    Task<bool> ExistsByNameAsync(string name, int? excludeAchievementTypeId = null);
}
