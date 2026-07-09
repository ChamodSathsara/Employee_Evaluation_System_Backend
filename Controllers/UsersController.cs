using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>User Management endpoints. Admin only, per the business rule that only Admin manages user accounts.</summary>
[Route("api/users")]
[Authorize(Roles = RoleNames.Admin)]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Lists users with optional keyword/role/department/status filters and paging.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserSearchDto search)
    {
        var result = await _userService.GetAllAsync(search);
        return Ok(ApiResponse<PagedResultDto<UserDto>>.SuccessResponse(result));
    }

    /// <summary>Returns a single user by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result));
    }

    /// <summary>Creates a new user account, validating the ReportsTo hierarchy for the assigned role.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.UserId },
            ApiResponse<UserDto>.SuccessResponse(result, "User created successfully."));
    }

    /// <summary>Updates an existing user's profile, role, department, designation, and ReportsTo.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _userService.UpdateUserAsync(id, request);
        return Ok(ApiResponse<UserDto>.SuccessResponse(result, "User updated successfully."));
    }

    /// <summary>Permanently deletes a user. Blocked if the user has direct reports or KPI/evaluation history — deactivate instead.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(ApiResponse.SuccessResponse("User deleted successfully."));
    }

    /// <summary>Activates or deactivates a user account.</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _userService.UpdateStatusAsync(id, request);
        var message = request.IsActive ? "User activated successfully." : "User deactivated successfully.";
        return Ok(ApiResponse<UserDto>.SuccessResponse(result, message));
    }

    /// <summary>Lists the direct reports (ReportsTo = userId) of the given user.</summary>
    [HttpGet("reporting-to/{userId:int}")]
    public async Task<IActionResult> GetReportingTo(int userId)
    {
        var result = await _userService.GetDirectReportsAsync(userId);
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(result));
    }

    /// <summary>Lists all users holding the given role.</summary>
    [HttpGet("by-role/{roleId:int}")]
    public async Task<IActionResult> GetByRole(int roleId)
    {
        var result = await _userService.GetByRoleAsync(roleId);
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(result));
    }

    /// <summary>Lists all users belonging to the given department.</summary>
    [HttpGet("by-department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var result = await _userService.GetByDepartmentAsync(departmentId);
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(result));
    }
}
