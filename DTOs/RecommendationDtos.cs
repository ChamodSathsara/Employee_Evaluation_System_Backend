namespace EmployeeEvaluationSystem.DTOs;

public record RecommendationDto(int RecommendationId, int ResultId, string RecommendationType, string? RecommendationNote);
public record CreateRecommendationDto(string RecommendationType, string? RecommendationNote);
