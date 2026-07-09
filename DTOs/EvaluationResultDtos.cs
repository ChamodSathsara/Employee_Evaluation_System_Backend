namespace EmployeeEvaluationSystem.DTOs;

public record EvaluationResultDto(
    int ResultId, int HODEvaluationId, decimal KPIScore, decimal CompetencyScore,
    decimal AchievementScore, decimal AttendancePenalty, decimal FinalScore, string Grade, DateTime ProcessedDate);
