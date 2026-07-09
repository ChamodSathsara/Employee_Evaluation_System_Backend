using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;

namespace EmployeeEvaluationSystem.Repositories;

public class RecommendationRepository : GenericRepository<Recommendation>, IRecommendationRepository
{
    public RecommendationRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement Recommendation-specific query methods declared on IRecommendationRepository
}
