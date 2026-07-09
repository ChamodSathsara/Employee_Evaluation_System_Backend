using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EmployeeEvaluationSystem.Helpers;

/// <summary>Reads the current caller's identity from the JWT claims attached to the current HTTP request.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? RoleName => Principal?.FindFirstValue(ClaimTypes.Role);

    public int? DepartmentId
    {
        get
        {
            var value = Principal?.FindFirstValue("departmentId");
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsInRole(string roleName) => Principal?.IsInRole(roleName) ?? false;
}
