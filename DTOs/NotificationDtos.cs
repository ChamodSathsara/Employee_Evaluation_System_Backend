namespace EmployeeEvaluationSystem.DTOs;

public record NotificationDto(int NotificationId, int UserId, string Title, string? Message, bool IsRead);
