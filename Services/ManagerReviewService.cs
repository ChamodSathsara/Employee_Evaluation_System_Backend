using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements Manager Review (SRS Step 15): the Manager reviews a processed evaluation
/// result belonging to an employee within their organizational subtree, and Approves,
/// Rejects, or Returns it (with a required reason on Return). A review record is created
/// once (Decision starts as "Pending") and then transitioned by exactly one of
/// Approve/Reject/Return.
/// </summary>
public class ManagerReviewService : IManagerReviewService
{
    private const string DecisionPending = "Pending";
    private const string DecisionApproved = "Approved";
    private const string DecisionRejected = "Rejected";
    private const string DecisionReturned = "Returned";

    private readonly IManagerReviewRepository _managerReviewRepository;
    private readonly IEvaluationResultRepository _evaluationResultRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public ManagerReviewService(
        IManagerReviewRepository managerReviewRepository,
        IEvaluationResultRepository evaluationResultRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _managerReviewRepository = managerReviewRepository;
        _evaluationResultRepository = evaluationResultRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<List<PendingManagerReviewDto>> GetPendingAsync(int managerId)
    {
        var subordinateIds = await GetSubordinateUserIdsAsync(managerId);
        var pending = await _evaluationResultRepository.GetPendingManagerReviewAsync(subordinateIds);

        return pending.Select(r => new PendingManagerReviewDto
        {
            ResultId = r.ResultId,
            HODEvaluationId = r.HODEvaluationId,
            EmployeeId = r.HODEvaluation.SelfEvaluation.EmployeeId,
            EmployeeName = $"{r.HODEvaluation.SelfEvaluation.Employee.FirstName} {r.HODEvaluation.SelfEvaluation.Employee.LastName}",
            FinalScore = r.FinalScore,
            Grade = r.Grade,
            ProcessedDate = r.ProcessedDate
        }).ToList();
    }

    public async Task<ManagerReviewDto> GetByResultAsync(int callerId, string callerRole, int resultId)
    {
        var review = await _managerReviewRepository.GetByResultAsync(resultId)
            ?? throw new NotFoundException("A Manager review has not been started for this result yet.");

        var full = await _managerReviewRepository.GetByIdWithDetailsAsync(review.ManagerReviewId);
        var allowed = callerRole == RoleNames.Admin
            || full!.ManagerId == callerId
            || full.Result.HODEvaluation.SelfEvaluation.EmployeeId == callerId;

        if (!allowed)
        {
            throw new ForbiddenException("You are not authorized to view this Manager review.");
        }

        return _mapper.Map<ManagerReviewDto>(full);
    }

    public async Task<ManagerReviewDto> CreateAsync(int managerId, CreateManagerReviewDto request)
    {
        var result = await _evaluationResultRepository.GetByIdWithDetailsAsync(request.ResultId)
            ?? throw new NotFoundException("Evaluation Result", request.ResultId);

        await ValidateManagerOwnsEmployeeAsync(managerId, result.HODEvaluation.SelfEvaluation.EmployeeId);

        if (await _managerReviewRepository.GetByResultAsync(request.ResultId) is not null)
        {
            throw new ConflictException("A Manager review already exists for this result.");
        }

        var review = new ManagerReview
        {
            ResultId = request.ResultId,
            ManagerId = managerId,
            Comments = request.Comments,
            Decision = DecisionPending,
            ReviewDate = DateTime.UtcNow
        };

        await _managerReviewRepository.AddAsync(review);
        await LogAuditAsync(managerId, "Create", "ManagerReviews", review.ManagerReviewId);
        await _managerReviewRepository.SaveChangesAsync();

        var created = await _managerReviewRepository.GetByIdWithDetailsAsync(review.ManagerReviewId);
        return _mapper.Map<ManagerReviewDto>(created!);
    }

    public async Task<ManagerReviewDto> UpdateAsync(int managerId, int managerReviewId, UpdateManagerReviewDto request)
    {
        var review = await _managerReviewRepository.GetByIdWithDetailsAsync(managerReviewId)
            ?? throw new NotFoundException("Manager Review", managerReviewId);

        ValidateManagerOwnsReview(managerId, review);

        review.Comments = request.Comments;
        await _managerReviewRepository.UpdateAsync(review);
        await LogAuditAsync(managerId, "Update", "ManagerReviews", managerReviewId);
        await _managerReviewRepository.SaveChangesAsync();

        return _mapper.Map<ManagerReviewDto>(review);
    }

    public async Task<ManagerReviewDto> ApproveAsync(int managerId, int managerReviewId, ManagerDecisionDto request)
    {
        return await TransitionDecisionAsync(managerId, managerReviewId, DecisionApproved, request.Comments,
            "Manager Approval", "Your evaluation has been approved and finalized by your Manager.");
    }

    public async Task<ManagerReviewDto> RejectAsync(int managerId, int managerReviewId, ManagerDecisionDto request)
    {
        return await TransitionDecisionAsync(managerId, managerReviewId, DecisionRejected, request.Comments,
            "Manager Approval", "Your evaluation has been rejected by your Manager.");
    }

    public async Task<ManagerReviewDto> ReturnAsync(int managerId, int managerReviewId, ReturnEvaluationDto request)
    {
        var review = await _managerReviewRepository.GetByIdWithDetailsAsync(managerReviewId)
            ?? throw new NotFoundException("Manager Review", managerReviewId);

        ValidateManagerOwnsReview(managerId, review);

        if (review.Decision != DecisionPending)
        {
            throw new ConflictException($"This review has already been decided ({review.Decision}) and cannot be returned.");
        }

        review.Decision = DecisionReturned;
        review.Comments = request.Reason;
        review.ReviewDate = DateTime.UtcNow;

        await _managerReviewRepository.UpdateAsync(review);
        await LogAuditAsync(managerId, "Manager Approval", "ManagerReviews", managerReviewId);

        var hodId = review.Result.HODEvaluation.HODId;
        await _notificationRepository.AddAsync(new Notification
        {
            UserId = hodId,
            Title = "Evaluation Returned by Manager",
            Message = $"The Manager returned an evaluation for correction. Reason: {request.Reason}",
            IsRead = false
        });

        await _managerReviewRepository.SaveChangesAsync();

        return _mapper.Map<ManagerReviewDto>(review);
    }

    // ---------- private helpers ----------

    private async Task<ManagerReviewDto> TransitionDecisionAsync(
        int managerId, int managerReviewId, string decision, string? comments, string auditAction, string notificationMessage)
    {
        var review = await _managerReviewRepository.GetByIdWithDetailsAsync(managerReviewId)
            ?? throw new NotFoundException("Manager Review", managerReviewId);

        ValidateManagerOwnsReview(managerId, review);

        if (review.Decision != DecisionPending)
        {
            throw new ConflictException($"This review has already been decided ({review.Decision}).");
        }

        review.Decision = decision;
        review.Comments = comments ?? review.Comments;
        review.ReviewDate = DateTime.UtcNow;

        await _managerReviewRepository.UpdateAsync(review);
        await LogAuditAsync(managerId, auditAction, "ManagerReviews", managerReviewId);

        var employeeId = review.Result.HODEvaluation.SelfEvaluation.EmployeeId;
        await _notificationRepository.AddAsync(new Notification
        {
            UserId = employeeId,
            Title = "Evaluation Decision Recorded",
            Message = notificationMessage,
            IsRead = false
        });

        await _managerReviewRepository.SaveChangesAsync();

        return _mapper.Map<ManagerReviewDto>(review);
    }

    private static void ValidateManagerOwnsReview(int managerId, ManagerReview review)
    {
        if (review.ManagerId != managerId)
        {
            throw new ForbiddenException("You can only act on your own Manager review.");
        }
    }

    private async Task ValidateManagerOwnsEmployeeAsync(int managerId, int employeeId)
    {
        var subordinateIds = await GetSubordinateUserIdsAsync(managerId);
        if (!subordinateIds.Contains(employeeId))
        {
            throw new ForbiddenException("This employee is not within your reporting hierarchy.");
        }
    }

    /// <summary>Walks the ReportsTo chain downward (BFS) from the Manager to find every employee in their organizational subtree.</summary>
    private async Task<HashSet<int>> GetSubordinateUserIdsAsync(int rootUserId)
    {
        var visited = new HashSet<int> { rootUserId };
        var queue = new Queue<int>();
        queue.Enqueue(rootUserId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var directReports = await _userRepository.GetDirectReportsAsync(currentId);

            foreach (var report in directReports)
            {
                if (visited.Add(report.UserId))
                {
                    queue.Enqueue(report.UserId);
                }
            }
        }

        return visited;
    }

    private async Task LogAuditAsync(int userId, string action, string tableName, int? recordId)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            TableName = tableName,
            RecordId = recordId,
            CreatedDate = DateTime.UtcNow
        });
    }
}
