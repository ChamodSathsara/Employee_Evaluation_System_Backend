using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<List<Notification>> GetByUserAsync(int userId);
    Task<List<Notification>> GetUnreadByUserAsync(int userId);
    Task MarkAllAsReadAsync(int userId);
}
