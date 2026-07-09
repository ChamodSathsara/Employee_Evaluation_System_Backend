using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Authentication and account-session endpoints: login, logout, token refresh, password change, and profile.</summary>
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    /// <summary>Authenticates a user and returns a JWT access token + refresh token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result, "Login successful."));
    }

    /// <summary>
    /// Creates a new user account. Restricted to Admin, consistent with the business rule
    /// that only Admin may create users. For hierarchy-validated user creation with full
    /// CRUD, prefer POST /api/users in the User Management module; this endpoint is a
    /// thin, self-contained alternative kept in the Auth module per module scope.
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = RoleNames.Admin)]
    //[AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Login), ApiResponse<UserProfileDto>.SuccessResponse(result, "User registered successfully."));
    }

    /// <summary>Rotates the access/refresh token pair.</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _authService.RefreshTokenAsync(request);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result, "Token refreshed successfully."));
    }

    /// <summary>Revokes the caller's refresh token.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        await _authService.LogoutAsync(userId);
        return Ok(ApiResponse.SuccessResponse("Logged out successfully."));
    }

    /// <summary>Changes the caller's own password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        await _authService.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse.SuccessResponse("Password changed successfully."));
    }

    /// <summary>Returns the caller's own profile.</summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var result = await _authService.GetProfileAsync(userId);
        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(result));
    }

    /// <summary>Returns 1st User.| for example </summary>
    [HttpGet("test")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTest()
    {
        //var result = await _authService.GetProfileAsync(userId);
        var result = await _authService.GetTest();
        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(result));
    }

    [HttpGet("db-check")]
    [AllowAnonymous]
    public async Task<IActionResult> DbCheck([FromServices] ApplicationDbContext context)
    {
        try
        {
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return Ok(new { connected = false, message = "CanConnectAsync returned false." });
            }

            var userCount = await context.Users.CountAsync();
            return Ok(new { connected = true, userCount });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                connected = false,
                error = ex.GetType().Name,
                message = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
    }


}
