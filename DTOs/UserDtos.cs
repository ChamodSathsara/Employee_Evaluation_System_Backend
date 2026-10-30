using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

/// <summary>Safe, flattened representation of a User returned to clients (never expose the User entity directly).</summary>
public class UserDto
{
    public int UserId { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public int? ReportsTo { get; set; }
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateUserDto
{
    [Required, MaxLength(50)]
    public string EmployeeNo { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    public int? DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    /// <summary>UserId of the direct manager. Required for every role except Admin and CEO (validated against the hierarchy rules in the service).</summary>
    public int? ReportsTo { get; set; }
}

public class UpdateUserDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    public int? DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    public int? ReportsTo { get; set; }
}

public class UpdateUserStatusDto
{
    [Required]
    public bool IsActive { get; set; }
}

/// <summary>Filter/search parameters for GET /api/users.</summary>
public class UserSearchDto
{
    public string? Keyword { get; set; }
    public int? RoleId { get; set; }
    public int? CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
