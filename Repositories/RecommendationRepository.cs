using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class RecommendationRepository : GenericRepository<Recommendation>, IRecommendationRepository
{
    public RecommendationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Recommendation>> GetByResultAsync(int resultId)
    {
        return await Context.Recommendations
            .Where(r => r.ResultId == resultId)
            .AsNoTracking()
            .ToListAsync();
    }
}
