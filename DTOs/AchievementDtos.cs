namespace EmployeeEvaluationSystem.DTOs;

public record AchievementTypeDto(int AchievementTypeId, string Name);
public record EmployeeAchievementDto(int AchievementId, int HODEvaluationId, int AchievementTypeId, string Title, decimal Marks, string? Attachment, string? Remarks);
public record CreateEmployeeAchievementDto(int AchievementTypeId, string Title, decimal Marks, string? Remarks);
