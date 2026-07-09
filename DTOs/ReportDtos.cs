namespace EmployeeEvaluationSystem.DTOs;

public record EmployeeReportRequestDto(int EmployeeId, int PeriodId, string Format);
public record DepartmentSummaryReportDto(int DepartmentId, int PeriodId, decimal AverageFinalScore, Dictionary<string, int> GradeDistribution);
