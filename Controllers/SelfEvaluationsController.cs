using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Employee Self Evaluation endpoints (SRS Step 6). Every authenticated employee manages only their own record.</summary>
[Route("api/self-evaluations")]
[Authorize]
public class SelfEvaluationsController : BaseApiController
{
    private readonly ISelfEvaluationService _selfEvaluationService;
    private readonly ICurrentUserService _currentUserService;

    public SelfEvaluationsController(ISelfEvaluationService selfEvaluationService, ICurrentUserService currentUserService)
    {
        _selfEvaluationService = selfEvaluationService;
        _currentUserService = currentUserService;
    }

    /// <summary>Returns the caller's self-evaluation for the given KPI assignment.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] int assignmentId)
    {
        var employeeId = RequireUserId();
        var result = await _selfEvaluationService.GetMyAsync(employeeId, assignmentId);
        return Ok(ApiResponse<EmployeeEvaluationDto>.SuccessResponse(result));
    }

    /// <summary>Returns the self-evaluation for a given KPI assignment (Admin, the employee, or the assignment's creator).</summary>
    [HttpGet("{assignmentId:int}")]
    public async Task<IActionResult> GetByAssignment(int assignmentId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _selfEvaluationService.GetByAssignmentAsync(callerId, callerRole, assignmentId);
        return result is null
            ? Ok(ApiResponse<EmployeeEvaluationDto?>.SuccessResponse(null, "No self-evaluation has been started for this assignment yet."))
            : Ok(ApiResponse<EmployeeEvaluationDto>.SuccessResponse(result));
    }

    /// <summary>Saves the caller's self-evaluation marks as a Draft (create or update).</summary>
    [HttpPost("save")]
    public async Task<IActionResult> SaveDraft([FromBody] SaveEmployeeEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var employeeId = RequireUserId();
        var result = await _selfEvaluationService.SaveDraftAsync(employeeId, request);
        return Ok(ApiResponse<EmployeeEvaluationDto>.SuccessResponse(result, "Self-evaluation saved as Draft."));
    }

    /// <summary>Submits the caller's self-evaluation, locking it from further edits.</summary>
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SaveEmployeeEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var employeeId = RequireUserId();
        var result = await _selfEvaluationService.SubmitAsync(employeeId, request);
        return Ok(ApiResponse<EmployeeEvaluationDto>.SuccessResponse(result, "Self-evaluation submitted successfully."));
    }

    /// <summary>Updates an existing Draft self-evaluation by id.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDraft(int id, [FromBody] SaveEmployeeEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var employeeId = RequireUserId();
        var result = await _selfEvaluationService.UpdateDraftAsync(employeeId, id, request);
        return Ok(ApiResponse<EmployeeEvaluationDto>.SuccessResponse(result, "Self-evaluation updated."));
    }

    /// <summary>Returns the caller's full self-evaluation history across all periods.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var employeeId = RequireUserId();
        var result = await _selfEvaluationService.GetMyHistoryAsync(employeeId);
        return Ok(ApiResponse<List<EmployeeEvaluationDto>>.SuccessResponse(result));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
