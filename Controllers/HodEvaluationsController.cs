using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>HOD Review endpoints (SRS Step 8). Restricted to the HOD role.</summary>
[Route("api/hod-evaluations")]
[Authorize(Roles = $"{RoleNames.HOD},{RoleNames.Admin}")]
public class HodEvaluationsController : BaseApiController
{
    private readonly IHodEvaluationService _hodEvaluationService;
    private readonly ICurrentUserService _currentUserService;

    public HodEvaluationsController(IHodEvaluationService hodEvaluationService, ICurrentUserService currentUserService)
    {
        _hodEvaluationService = hodEvaluationService;
        _currentUserService = currentUserService;
    }

    /// <summary>Returns the caller's pending HOD-review queue.</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var hodId = RequireUserId();
        var result = await _hodEvaluationService.GetPendingAsync(hodId);
        return Ok(ApiResponse<List<PendingReviewDto>>.SuccessResponse(result));
    }

    /// <summary>Returns a single HOD evaluation by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _hodEvaluationService.GetByIdAsync(callerId, callerRole, id);
        return Ok(ApiResponse<HodEvaluationDto>.SuccessResponse(result));
    }

    /// <summary>Starts a new HOD review (Draft) for a Submitted self-evaluation.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHodEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _hodEvaluationService.CreateAsync(hodId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.HODEvaluationId },
            ApiResponse<HodEvaluationDto>.SuccessResponse(result, "HOD evaluation created as Draft."));
    }

    /// <summary>Updates the marks/comments of a Draft HOD evaluation.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHodEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _hodEvaluationService.UpdateAsync(hodId, id, request);
        return Ok(ApiResponse<HodEvaluationDto>.SuccessResponse(result, "HOD evaluation updated."));
    }

    /// <summary>Submits the HOD evaluation, locking it and making it eligible for grade processing.</summary>
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var hodId = RequireUserId();
        var result = await _hodEvaluationService.SubmitAsync(hodId, id);
        return Ok(ApiResponse<HodEvaluationDto>.SuccessResponse(result, "HOD evaluation submitted successfully."));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
