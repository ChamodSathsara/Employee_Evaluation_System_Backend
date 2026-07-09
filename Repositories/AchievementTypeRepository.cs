using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;

namespace EmployeeEvaluationSystem.Repositories;

public class AchievementTypeRepository : GenericRepository<AchievementType>, IAchievementTypeRepository
{
    public AchievementTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement AchievementType-specific query methods declared on IAchievementTypeRepository
}
