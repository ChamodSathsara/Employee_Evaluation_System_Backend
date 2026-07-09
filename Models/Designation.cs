namespace EmployeeEvaluationSystem.Models;

public class Designation
{
    public int DesignationId { get; set; }
    public string DesignationName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
