namespace EmployeeEvaluationSystem.Models;

public class AuditLog
{
    public int AuditId { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int? RecordId { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
