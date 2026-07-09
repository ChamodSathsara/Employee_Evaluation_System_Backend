namespace EmployeeEvaluationSystem.DTOs;

public record AttendanceSummaryDto(
    int AttendanceId, int EmployeeId, int PeriodId,
    decimal AnnualLeave, decimal CasualLeave, decimal MedicalLeave, decimal NoPayLeave,
    int LateAttendance, decimal PenaltyMarks);

public record UpsertAttendanceSummaryDto(
    int EmployeeId, int PeriodId,
    decimal AnnualLeave, decimal CasualLeave, decimal MedicalLeave, decimal NoPayLeave,
    int LateAttendance);
