namespace EmployeeEvaluationSystem.Models;

// Generic/polymorphic attachment: RelatedTable + RelatedId identify the owning
// record (e.g. RelatedTable = "EmployeeAchievements", RelatedId = AchievementId)
// rather than a strict foreign key, so this entity intentionally has no navigation
// property to a single parent type.
public class Attachment
{
    public int AttachmentId { get; set; }
    public string RelatedTable { get; set; } = string.Empty;
    public int RelatedId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}
