using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IAttendanceSummaryRepository : IGenericRepository<AttendanceSummary>
{
    Task<AttendanceSummary?> GetByEmployeeAndPeriodAsync(int employeeId, int periodId);
    Task<List<AttendanceSummary>> GetByEmployeeAsync(int employeeId);
    Task<List<AttendanceSummary>> GetByPeriodAsync(int periodId);
}
