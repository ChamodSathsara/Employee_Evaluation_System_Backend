using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class AchievementTypeDto
{
    public int AchievementTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class EmployeeAchievementDto
{
    public int AchievementId { get; set; }
    public int HODEvaluationId { get; set; }
    public int AchievementTypeId { get; set; }
    public string AchievementTypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Marks { get; set; }
    public string? Attachment { get; set; }
    public string? Remarks { get; set; }
}

public class CreateEmployeeAchievementDto
{
    [Required]
    public int HODEvaluationId { get; set; }

    [Required]
    public int AchievementTypeId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, Range(0, 100)]
    public decimal Marks { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public class UpdateEmployeeAchievementDto
{
    [Required]
    public int AchievementTypeId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, Range(0, 100)]
    public decimal Marks { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public class UploadAchievementAttachmentDto
{
    [Required]
    public int AchievementId { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}
