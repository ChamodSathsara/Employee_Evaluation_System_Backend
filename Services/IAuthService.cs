using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IAuthService
{
    /// <summary>Authenticates a user by email/password and issues a new access + refresh token pair.</summary>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Creates a new user account and returns their profile. Restricted to Admin at the
    /// controller level (see AuthController); kept here as a thin, self-contained
    /// account-creation path distinct from the richer, hierarchy-validated
    /// IUserService.CreateUserAsync used by the User Management module.
    /// </summary>
    Task<UserProfileDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>Rotates the access/refresh token pair given a valid (possibly expired) access token and its matching refresh token.</summary>
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

    /// <summary>Revokes the caller's stored refresh token, ending their ability to silently refresh.</summary>
    Task LogoutAsync(int userId);

    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);

    Task<UserProfileDto> GetProfileAsync(int userId);

    Task<UserProfileDto> GetTest();
}
