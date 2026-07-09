namespace EmployeeEvaluationSystem.Models;

public class AchievementType
{
    public int AchievementTypeId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<EmployeeAchievement> EmployeeAchievements { get; set; } = new List<EmployeeAchievement>();
}
