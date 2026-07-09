using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public class RecommendationRepository : GenericRepository<Recommendation>, IRecommendationRepository
{
    public RecommendationRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement Recommendation-specific query methods declared on IRecommendationRepository
}
