namespace EmployeeEvaluationSystem.DTOs;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool IsRead { get; set; }
}
