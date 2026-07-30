using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Notification>> GetByUserAsync(int userId)
    {
        return await Context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.NotificationId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Notification>> GetUnreadByUserAsync(int userId)
    {
        return await Context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.NotificationId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var unread = await Context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }
    }
}
