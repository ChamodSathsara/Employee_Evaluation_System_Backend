using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Manager Review endpoints (SRS Step 15). Restricted to the Manager role.</summary>
[Route("api/manager-reviews")]
[Authorize(Roles = $"{RoleNames.Manager},{RoleNames.Admin}")]
public class ManagerReviewsController : BaseApiController
{
    private readonly IManagerReviewService _managerReviewService;
    private readonly ICurrentUserService _currentUserService;

    public ManagerReviewsController(IManagerReviewService managerReviewService, ICurrentUserService currentUserService)
    {
        _managerReviewService = managerReviewService;
        _currentUserService = currentUserService;
    }

    /// <summary>Lists processed evaluation results awaiting Manager decision, within the caller's organizational subtree.</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var managerId = RequireUserId();
        var result = await _managerReviewService.GetPendingAsync(managerId);
        return Ok(ApiResponse<List<PendingManagerReviewDto>>.SuccessResponse(result));
    }

    /// <summary>Returns the Manager review record for a given evaluation result.</summary>
    [HttpGet("{resultId:int}")]
    public async Task<IActionResult> GetByResult(int resultId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _managerReviewService.GetByResultAsync(callerId, callerRole, resultId);
        return Ok(ApiResponse<ManagerReviewDto>.SuccessResponse(result));
    }

    /// <summary>Starts a Manager review (Decision = Pending) for a processed evaluation result.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManagerReviewDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var managerId = RequireUserId();
        var result = await _managerReviewService.CreateAsync(managerId, request);
        return Ok(ApiResponse<ManagerReviewDto>.SuccessResponse(result, "Manager review started."));
    }

    /// <summary>Updates the comments on a pending Manager review.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManagerReviewDto request)
    {
        var managerId = RequireUserId();
        var result = await _managerReviewService.UpdateAsync(managerId, id, request);
        return Ok(ApiResponse<ManagerReviewDto>.SuccessResponse(result, "Manager review updated."));
    }

    /// <summary>Approves and finalizes the evaluation.</summary>
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ManagerDecisionDto request)
    {
        var managerId = RequireUserId();
        var result = await _managerReviewService.ApproveAsync(managerId, id, request);
        return Ok(ApiResponse<ManagerReviewDto>.SuccessResponse(result, "Evaluation approved."));
    }

    /// <summary>Rejects the evaluation.</summary>
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ManagerDecisionDto request)
    {
        var managerId = RequireUserId();
        var result = await _managerReviewService.RejectAsync(managerId, id, request);
        return Ok(ApiResponse<ManagerReviewDto>.SuccessResponse(result, "Evaluation rejected."));
    }

    /// <summary>Returns the evaluation to the HOD with a required reason.</summary>
    [HttpPost("{id:int}/return")]
    public async Task<IActionResult> Return(int id, [FromBody] ReturnEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("A reason is required to return an evaluation."));
        }

        var managerId = RequireUserId();
        var result = await _managerReviewService.ReturnAsync(managerId, id, request);
        return Ok(ApiResponse<ManagerReviewDto>.SuccessResponse(result, "Evaluation returned to HOD."));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
