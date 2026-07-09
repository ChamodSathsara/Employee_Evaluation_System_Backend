namespace EmployeeEvaluationSystem.Models;

public class Notification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool IsRead { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
