using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public class AttendanceSummaryRepository : GenericRepository<AttendanceSummary>, IAttendanceSummaryRepository
{
    public AttendanceSummaryRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement AttendanceSummary-specific query methods declared on IAttendanceSummaryRepository
}
