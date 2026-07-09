using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;

namespace EmployeeEvaluationSystem.Repositories;

public class AttendanceSummaryRepository : GenericRepository<AttendanceSummary>, IAttendanceSummaryRepository
{
    public AttendanceSummaryRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement AttendanceSummary-specific query methods declared on IAttendanceSummaryRepository
}
