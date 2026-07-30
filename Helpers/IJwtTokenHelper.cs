using System.Security.Claims;

namespace EmployeeEvaluationSystem.Helpers;

public interface IJwtTokenHelper
{
    /// <summary>Generates a signed, short-lived JWT access token carrying the user's identity, role, and hierarchy claims.</summary>
    string GenerateAccessToken(int userId, string email, string roleName, int? reportsTo, int? departmentId);

    /// <summary>Generates a cryptographically random refresh token (opaque string, not a JWT).</summary>
    string GenerateRefreshToken();

    /// <summary>Computes a SHA-256 hash of a refresh token for safe storage/comparison at rest.</summary>
    string HashRefreshToken(string refreshToken);

    /// <summary>Extracts the ClaimsPrincipal from an access token even if it has already expired (used to validate a refresh-token request).</summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
