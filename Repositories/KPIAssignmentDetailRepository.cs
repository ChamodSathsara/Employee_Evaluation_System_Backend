using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public class KPIAssignmentDetailRepository : GenericRepository<KPIAssignmentDetail>, IKPIAssignmentDetailRepository
{
    public KPIAssignmentDetailRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement KPIAssignmentDetail-specific query methods declared on IKPIAssignmentDetailRepository
}
