using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class CompetencyEvaluationRepository : GenericRepository<CompetencyEvaluation>, ICompetencyEvaluationRepository
{
    public CompetencyEvaluationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<CompetencyEvaluation>> GetByHodEvaluationAsync(int hodEvaluationId)
    {
        return await Context.CompetencyEvaluations
            .Include(ce => ce.Competency)
            .Where(ce => ce.HODEvaluationId == hodEvaluationId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CompetencyEvaluation?> GetByHodEvaluationAndCompetencyAsync(int hodEvaluationId, int competencyId)
    {
        return await Context.CompetencyEvaluations
            .FirstOrDefaultAsync(ce => ce.HODEvaluationId == hodEvaluationId && ce.CompetencyId == competencyId);
    }
}
