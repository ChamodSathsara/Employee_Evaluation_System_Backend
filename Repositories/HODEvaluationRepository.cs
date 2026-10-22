using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class HODEvaluationRepository : GenericRepository<HODEvaluation>, IHODEvaluationRepository
{
    public HODEvaluationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<HODEvaluation?> GetByIdWithDetailsAsync(int hodEvaluationId)
    {
        return await Context.HodEvaluations
            .Include(h => h.HOD)
            .Include(h => h.SelfEvaluation).ThenInclude(e => e.Employee)
            .Include(h => h.SelfEvaluation).ThenInclude(e => e.Assignment)
            .Include(h => h.SelfEvaluation).ThenInclude(e => e.Details)
            .Include(h => h.SelfEvaluation).ThenInclude(e => e.SupervisorEvaluations).ThenInclude(s => s.Details)
            .Include(h => h.Details).ThenInclude(d => d.KpiAssignmentDetail)
            .AsSplitQuery()
            .FirstOrDefaultAsync(h => h.HODEvaluationId == hodEvaluationId);
    }

    public async Task<HODEvaluation?> GetBySelfEvaluationAsync(int selfEvaluationId)
    {
        return await Context.HodEvaluations
            .Include(h => h.Details)
            .FirstOrDefaultAsync(h => h.SelfEvaluationId == selfEvaluationId);
    }

    public async Task ReplaceDetailsAsync(int hodEvaluationId, List<HODEvaluationDetail> newDetails)
    {
        var existing = await Context.HodEvaluationDetails
            .Where(d => d.HODEvaluationId == hodEvaluationId)
            .ToListAsync();

        Context.HodEvaluationDetails.RemoveRange(existing);

        foreach (var detail in newDetails)
        {
            detail.HODEvaluationId = hodEvaluationId;
        }

        await Context.HodEvaluationDetails.AddRangeAsync(newDetails);
    }
}
