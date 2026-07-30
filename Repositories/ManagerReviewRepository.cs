using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class ManagerReviewRepository : GenericRepository<ManagerReview>, IManagerReviewRepository
{
    public ManagerReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ManagerReview?> GetByResultAsync(int resultId)
    {
        return await Context.ManagerReviews
            .Include(m => m.Manager)
            .FirstOrDefaultAsync(m => m.ResultId == resultId);
    }

    public async Task<ManagerReview?> GetByIdWithDetailsAsync(int managerReviewId)
    {
        return await Context.ManagerReviews
            .Include(m => m.Manager)
            .Include(m => m.Result).ThenInclude(r => r.HODEvaluation).ThenInclude(h => h.SelfEvaluation).ThenInclude(e => e.Employee)
            .FirstOrDefaultAsync(m => m.ManagerReviewId == managerReviewId);
    }
}
