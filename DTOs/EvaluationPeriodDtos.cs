namespace EmployeeEvaluationSystem.DTOs;

public record EvaluationPeriodDto(int PeriodId, string Name, int Year, DateOnly StartDate, DateOnly EndDate, string Status);
public record CreateEvaluationPeriodDto(string Name, int Year, DateOnly StartDate, DateOnly EndDate);
