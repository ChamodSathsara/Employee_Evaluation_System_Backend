using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IAchievementService
{
    Task<List<EmployeeAchievementDto>> GetByHodEvaluationAsync(int callerId, string callerRole, int hodEvaluationId);
    Task<EmployeeAchievementDto> CreateAsync(int hodId, CreateEmployeeAchievementDto request);
    Task<EmployeeAchievementDto> UpdateAsync(int hodId, int achievementId, UpdateEmployeeAchievementDto request);
    Task DeleteAsync(int hodId, int achievementId);
    Task<EmployeeAchievementDto> UploadAttachmentAsync(int hodId, int achievementId, string storedFilePath);
}
