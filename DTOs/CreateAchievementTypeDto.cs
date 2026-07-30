using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

// AchievementTypeDto already exists in AchievementDtos.cs - reused here, not redefined.

public class CreateAchievementTypeDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateAchievementTypeDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
