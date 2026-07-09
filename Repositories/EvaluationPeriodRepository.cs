using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public class EvaluationPeriodRepository : GenericRepository<EvaluationPeriod>, IEvaluationPeriodRepository
{
    public EvaluationPeriodRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement EvaluationPeriod-specific query methods declared on IEvaluationPeriodRepository
}
