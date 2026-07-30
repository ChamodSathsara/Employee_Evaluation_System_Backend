using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements HOD Recommendations (SRS Step 14): Promotion, Salary Increment, Training,
/// Extend Probation, or No Increment, with an optional note. Only the HOD who owns the
/// underlying HOD evaluation may add/edit recommendations, and only after the evaluation
/// has been processed (an EvaluationResult exists).
/// </summary>
public class RecommendationService : IRecommendationService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Promotion", "Salary Increment", "Training", "Extend Probation", "No Increment"
    };

    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IEvaluationResultRepository _evaluationResultRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public RecommendationService(
        IRecommendationRepository recommendationRepository,
        IEvaluationResultRepository evaluationResultRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _recommendationRepository = recommendationRepository;
        _evaluationResultRepository = evaluationResultRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<List<RecommendationDto>> GetByResultAsync(int callerId, string callerRole, int resultId)
    {
        var result = await _evaluationResultRepository.GetByIdWithDetailsAsync(resultId)
            ?? throw new NotFoundException("Evaluation Result", resultId);

        var allowed = callerRole == RoleNames.Admin
            || result.HODEvaluation.HODId == callerId
            || result.HODEvaluation.SelfEvaluation.EmployeeId == callerId;

        if (!allowed)
        {
            throw new ForbiddenException("You are not authorized to view these recommendations.");
        }

        var recommendations = await _recommendationRepository.GetByResultAsync(resultId);
        return _mapper.Map<List<RecommendationDto>>(recommendations);
    }

    public async Task<RecommendationDto> CreateAsync(int hodId, int resultId, CreateRecommendationDto request)
    {
        var result = await ValidateHodOwnsResultAsync(hodId, resultId);
        ValidateRecommendationType(request.RecommendationType);

        var recommendation = new Recommendation
        {
            ResultId = result.ResultId,
            RecommendationType = request.RecommendationType,
            RecommendationNote = request.RecommendationNote
        };

        await _recommendationRepository.AddAsync(recommendation);
        await LogAuditAsync(hodId, "Create", "Recommendations", recommendation.RecommendationId);
        await _recommendationRepository.SaveChangesAsync();

        return _mapper.Map<RecommendationDto>(recommendation);
    }

    public async Task<RecommendationDto> UpdateAsync(int hodId, int recommendationId, UpdateRecommendationDto request)
    {
        var recommendation = await _recommendationRepository.GetByIdAsync(recommendationId)
            ?? throw new NotFoundException("Recommendation", recommendationId);

        await ValidateHodOwnsResultAsync(hodId, recommendation.ResultId);
        ValidateRecommendationType(request.RecommendationType);

        recommendation.RecommendationType = request.RecommendationType;
        recommendation.RecommendationNote = request.RecommendationNote;

        await _recommendationRepository.UpdateAsync(recommendation);
        await LogAuditAsync(hodId, "Update", "Recommendations", recommendationId);
        await _recommendationRepository.SaveChangesAsync();

        return _mapper.Map<RecommendationDto>(recommendation);
    }

    private async Task<EvaluationResult> ValidateHodOwnsResultAsync(int hodId, int resultId)
    {
        var result = await _evaluationResultRepository.GetByIdWithDetailsAsync(resultId)
            ?? throw new NotFoundException("Evaluation Result", resultId);

        if (result.HODEvaluation.HODId != hodId)
        {
            throw new ForbiddenException("You can only add recommendations to your own processed evaluations.");
        }

        return result;
    }

    private static void ValidateRecommendationType(string recommendationType)
    {
        if (!AllowedTypes.Contains(recommendationType))
        {
            throw new ValidationAppException(
                $"'{recommendationType}' is not a valid recommendation type. Allowed values: {string.Join(", ", AllowedTypes)}.");
        }
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
