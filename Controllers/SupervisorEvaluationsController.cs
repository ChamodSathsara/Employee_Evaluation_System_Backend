using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Supervisor Review endpoints (SRS Step 7, optional). Restricted to the Supervisor role.</summary>
[Route("api/supervisor-evaluations")]
[Authorize(Roles = $"{RoleNames.Supervisor},{RoleNames.Admin}")]
public class SupervisorEvaluationsController : BaseApiController
{
    private readonly ISupervisorEvaluationService _supervisorEvaluationService;
    private readonly ICurrentUserService _currentUserService;

    public SupervisorEvaluationsController(ISupervisorEvaluationService supervisorEvaluationService, ICurrentUserService currentUserService)
    {
        _supervisorEvaluationService = supervisorEvaluationService;
        _currentUserService = currentUserService;
    }

    /// <summary>Returns the caller's pending Supervisor-review queue (direct reports who have Submitted their self-evaluation).</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var supervisorId = RequireUserId();
        var result = await _supervisorEvaluationService.GetPendingAsync(supervisorId);
        return Ok(ApiResponse<List<PendingReviewDto>>.SuccessResponse(result));
    }

    /// <summary>Returns a single supervisor evaluation by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _supervisorEvaluationService.GetByIdAsync(callerId, callerRole, id);
        return Ok(ApiResponse<SupervisorEvaluationDto>.SuccessResponse(result));
    }

    /// <summary>Starts a new Supervisor review (Draft) for a Submitted self-evaluation.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupervisorEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var supervisorId = RequireUserId();
        var result = await _supervisorEvaluationService.CreateAsync(supervisorId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.SupervisorEvaluationId },
            ApiResponse<SupervisorEvaluationDto>.SuccessResponse(result, "Supervisor evaluation created as Draft."));
    }

    /// <summary>Updates the marks/comments of a Draft Supervisor evaluation.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupervisorEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var supervisorId = RequireUserId();
        var result = await _supervisorEvaluationService.UpdateAsync(supervisorId, id, request);
        return Ok(ApiResponse<SupervisorEvaluationDto>.SuccessResponse(result, "Supervisor evaluation updated."));
    }

    /// <summary>Submits the Supervisor evaluation, locking it and making it visible to the HOD.</summary>
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var supervisorId = RequireUserId();
        var result = await _supervisorEvaluationService.SubmitAsync(supervisorId, id);
        return Ok(ApiResponse<SupervisorEvaluationDto>.SuccessResponse(result, "Supervisor evaluation submitted successfully."));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
