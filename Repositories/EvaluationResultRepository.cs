using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public class EvaluationResultRepository : GenericRepository<EvaluationResult>, IEvaluationResultRepository
{
    public EvaluationResultRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement EvaluationResult-specific query methods declared on IEvaluationResultRepository
}
