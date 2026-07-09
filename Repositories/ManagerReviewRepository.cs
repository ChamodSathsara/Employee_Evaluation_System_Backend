using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;

namespace EmployeeEvaluationSystem.Repositories;

public class ManagerReviewRepository : GenericRepository<ManagerReview>, IManagerReviewRepository
{
    public ManagerReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement ManagerReview-specific query methods declared on IManagerReviewRepository
}
