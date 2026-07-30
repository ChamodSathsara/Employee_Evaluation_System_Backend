using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetAllAsync(int userId);
    Task<List<NotificationDto>> GetUnreadAsync(int userId);
    Task<NotificationDto> MarkAsReadAsync(int userId, int notificationId);
    Task MarkAllAsReadAsync(int userId);
}
