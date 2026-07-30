using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class EvaluationResultRepository : GenericRepository<EvaluationResult>, IEvaluationResultRepository
{
    public EvaluationResultRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<EvaluationResult?> GetByHodEvaluationAsync(int hodEvaluationId)
    {
        return await Context.EvaluationResults
            .FirstOrDefaultAsync(r => r.HODEvaluationId == hodEvaluationId);
    }

    public async Task<EvaluationResult?> GetByIdWithDetailsAsync(int resultId)
    {
        return await Context.EvaluationResults
            .Include(r => r.HODEvaluation).ThenInclude(h => h.SelfEvaluation).ThenInclude(e => e.Employee)
            .Include(r => r.Recommendations)
            .Include(r => r.ManagerReviews)
            .FirstOrDefaultAsync(r => r.ResultId == resultId);
    }

    public async Task<List<EvaluationResult>> GetByEmployeeAsync(int employeeId)
    {
        return await Context.EvaluationResults
            .Include(r => r.HODEvaluation).ThenInclude(h => h.SelfEvaluation).ThenInclude(e => e.Employee)
            .Where(r => r.HODEvaluation.SelfEvaluation.EmployeeId == employeeId)
            .OrderByDescending(r => r.ProcessedDate)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<EvaluationResult>> GetByPeriodAsync(int periodId)
    {
        return await Context.EvaluationResults
            .Include(r => r.HODEvaluation).ThenInclude(h => h.SelfEvaluation).ThenInclude(e => e.Employee)
            .Include(r => r.HODEvaluation).ThenInclude(h => h.SelfEvaluation).ThenInclude(e => e.Assignment)
            .Where(r => r.HODEvaluation.SelfEvaluation.Assignment.PeriodId == periodId)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<EvaluationResult>> GetPendingManagerReviewAsync(IReadOnlySet<int> subordinateEmployeeIds)
    {
        return await Context.EvaluationResults
            .Include(r => r.HODEvaluation).ThenInclude(h => h.SelfEvaluation).ThenInclude(e => e.Employee)
            .Where(r => subordinateEmployeeIds.Contains(r.HODEvaluation.SelfEvaluation.EmployeeId))
            .Where(r => !r.ManagerReviews.Any())
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }
}
