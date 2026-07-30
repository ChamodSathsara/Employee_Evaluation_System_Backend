using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Notification endpoints. Every authenticated user manages only their own notifications.</summary>
[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    /// <summary>Lists all notifications for the caller.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = RequireUserId();
        var result = await _notificationService.GetAllAsync(userId);
        return Ok(ApiResponse<List<NotificationDto>>.SuccessResponse(result));
    }

    /// <summary>Lists only unread notifications for the caller.</summary>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var userId = RequireUserId();
        var result = await _notificationService.GetUnreadAsync(userId);
        return Ok(ApiResponse<List<NotificationDto>>.SuccessResponse(result));
    }

    /// <summary>Marks a single notification as read.</summary>
    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = RequireUserId();
        var result = await _notificationService.MarkAsReadAsync(userId, id);
        return Ok(ApiResponse<NotificationDto>.SuccessResponse(result, "Notification marked as read."));
    }

    /// <summary>Marks every notification for the caller as read.</summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = RequireUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(ApiResponse.SuccessResponse("All notifications marked as read."));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();
}
