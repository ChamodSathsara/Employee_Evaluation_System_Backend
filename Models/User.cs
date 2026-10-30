namespace EmployeeEvaluationSystem.Models;

public class User
{
    public int UserId { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int? CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? ReportedSupervisorId { get; set; }
    public int? ReportedHodId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    // Refresh-token storage. The schema has no dedicated RefreshTokens table, so the
    // (hashed) current refresh token and its expiry are kept directly on the user row.
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation properties
    public Role Role { get; set; } = null!;
    public Company? Company { get; set; }
    public Department? Department { get; set; }
    public Designation? Designation { get; set; }
    public User? ReportedSupervisor { get; set; }
    public User? ReportedHod { get; set; }
    public ICollection<User> SupervisorReports { get; set; } = new List<User>();
    public ICollection<User> HodReports { get; set; } = new List<User>();
}
