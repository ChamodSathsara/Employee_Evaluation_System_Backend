using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IManagerReviewRepository : IGenericRepository<ManagerReview>
{
    Task<ManagerReview?> GetByResultAsync(int resultId);
    Task<ManagerReview?> GetByIdWithDetailsAsync(int managerReviewId);
}
