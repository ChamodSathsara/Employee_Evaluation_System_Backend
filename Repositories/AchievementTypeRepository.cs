using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class AchievementTypeRepository : GenericRepository<AchievementType>, IAchievementTypeRepository
{
    public AchievementTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeAchievementTypeId = null)
    {
        var query = Context.AchievementTypes.Where(a => a.Name.ToLower() == name.ToLower());
        if (excludeAchievementTypeId.HasValue)
        {
            query = query.Where(a => a.AchievementTypeId != excludeAchievementTypeId.Value);
        }

        return await query.AnyAsync();
    }
}
