using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class EmployeeAchievementRepository : GenericRepository<EmployeeAchievement>, IEmployeeAchievementRepository
{
    public EmployeeAchievementRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<EmployeeAchievement>> GetByHodEvaluationAsync(int hodEvaluationId)
    {
        return await Context.EmployeeAchievements
            .Include(a => a.AchievementType)
            .Where(a => a.HODEvaluationId == hodEvaluationId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<decimal> GetTotalMarksAsync(int hodEvaluationId, int? excludeAchievementId = null)
    {
        var query = Context.EmployeeAchievements.Where(a => a.HODEvaluationId == hodEvaluationId);
        if (excludeAchievementId.HasValue)
        {
            query = query.Where(a => a.AchievementId != excludeAchievementId.Value);
        }

        return await query.SumAsync(a => (decimal?)a.Marks) ?? 0;
    }
}
