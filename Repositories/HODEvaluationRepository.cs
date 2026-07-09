using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;

namespace EmployeeEvaluationSystem.Repositories;

public class HODEvaluationRepository : GenericRepository<HODEvaluation>, IHODEvaluationRepository
{
    public HODEvaluationRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement HODEvaluation-specific query methods declared on IHODEvaluationRepository
}
