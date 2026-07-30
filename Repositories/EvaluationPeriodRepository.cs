using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class EvaluationPeriodRepository : GenericRepository<EvaluationPeriod>, IEvaluationPeriodRepository
{
    public EvaluationPeriodRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<EvaluationPeriod?> GetActiveAsync()
    {
        return await Context.EvaluationPeriods.FirstOrDefaultAsync(p => p.Status == "Active");
    }

    public async Task<List<EvaluationPeriod>> GetAllOrderedAsync()
    {
        return await Context.EvaluationPeriods
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.StartDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludePeriodId = null)
    {
        var query = Context.EvaluationPeriods.Where(p => p.Name.ToLower() == name.ToLower());
        if (excludePeriodId.HasValue)
        {
            query = query.Where(p => p.PeriodId != excludePeriodId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task DeactivateAllExceptAsync(int exceptPeriodId)
    {
        var activePeriods = await Context.EvaluationPeriods
            .Where(p => p.Status == "Active" && p.PeriodId != exceptPeriodId)
            .ToListAsync();

        foreach (var period in activePeriods)
        {
            period.Status = "Inactive";
        }
    }

    public async Task<bool> HasKpiAssignmentsAsync(int periodId)
    {
        return await Context.KpiAssignments.AnyAsync(a => a.PeriodId == periodId);
    }
}
