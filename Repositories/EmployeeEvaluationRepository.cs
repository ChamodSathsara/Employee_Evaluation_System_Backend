using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public class EmployeeEvaluationRepository : GenericRepository<EmployeeEvaluation>, IEmployeeEvaluationRepository
{
    public EmployeeEvaluationRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement EmployeeEvaluation-specific query methods declared on IEmployeeEvaluationRepository
}
