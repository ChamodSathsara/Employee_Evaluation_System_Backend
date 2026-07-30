using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IAttendanceService
{
    Task<List<AttendanceSummaryDto>> GetByEmployeeAsync(int callerId, string callerRole, int employeeId);
    Task<AttendanceSummaryDto> UpsertAsync(int callerId, UpsertAttendanceSummaryDto request);
    Task<AttendanceSummaryDto> UpdateAsync(int callerId, int attendanceId, UpsertAttendanceSummaryDto request);
    Task<int> ImportAsync(int callerId, ImportAttendanceDto request);
    Task<List<AttendanceSummaryDto>> GetSummaryByPeriodAsync(int periodId);
}
