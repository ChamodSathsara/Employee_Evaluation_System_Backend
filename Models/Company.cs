namespace EmployeeEvaluationSystem.Models;

public class Company
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
