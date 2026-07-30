using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IManagerReviewService
{
    Task<List<PendingManagerReviewDto>> GetPendingAsync(int managerId);
    Task<ManagerReviewDto> GetByResultAsync(int callerId, string callerRole, int resultId);
    Task<ManagerReviewDto> CreateAsync(int managerId, CreateManagerReviewDto request);
    Task<ManagerReviewDto> UpdateAsync(int managerId, int managerReviewId, UpdateManagerReviewDto request);
    Task<ManagerReviewDto> ApproveAsync(int managerId, int managerReviewId, ManagerDecisionDto request);
    Task<ManagerReviewDto> RejectAsync(int managerId, int managerReviewId, ManagerDecisionDto request);
    Task<ManagerReviewDto> ReturnAsync(int managerId, int managerReviewId, ReturnEvaluationDto request);
}
