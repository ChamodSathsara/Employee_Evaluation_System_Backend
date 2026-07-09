using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;

namespace EmployeeEvaluationSystem.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement Notification-specific query methods declared on INotificationRepository
}
