using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class CompetencyRepository : GenericRepository<Competency>, ICompetencyRepository
{
    public CompetencyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeCompetencyId = null)
    {
        var query = Context.Competencies.Where(c => c.Name.ToLower() == name.ToLower());
        if (excludeCompetencyId.HasValue)
        {
            query = query.Where(c => c.CompetencyId != excludeCompetencyId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> IsInUseAsync(int competencyId)
    {
        return await Context.CompetencyEvaluations.AnyAsync(ce => ce.CompetencyId == competencyId);
    }
}
