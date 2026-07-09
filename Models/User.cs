using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeEvaluationSystem.Models;

[Table("Users", Schema = "dbo")]
public class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Column("EmployeeNo")]
    [MaxLength(50)]
    public string EmployeeNo { get; set; } = string.Empty;

    [Column("FirstName")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Column("LastName")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Column("Email")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("RoleID")]
    public int RoleId { get; set; }

    [Column("DepartmentID")]
    public int DepartmentId { get; set; }

    [Column("DesignationID")]
    public int DesignationId { get; set; }

    [Column("ReportsTo")]
    public int? ReportsTo { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; }

    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    // Refresh-token storage. The schema has no dedicated RefreshTokens table, so the
    // (hashed) current refresh token and its expiry are kept directly on the user row.
    [Column("RefreshTokenHash")]
    public string? RefreshTokenHash { get; set; }

    [Column("RefreshTokenExpiryTime")]
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public Role Role { get; set; } = null!;

    [ForeignKey("DepartmentId")]
    public Department Department { get; set; } = null!;

    [ForeignKey("DesignationId")]
    public Designation Designation { get; set; } = null!;

    [ForeignKey("ReportsTo")]
    public User? Manager { get; set; }

    public ICollection<User> DirectReports { get; set; } = new List<User>();
}