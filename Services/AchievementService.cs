using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements Special Achievements (SRS Step 10): the HOD records achievement entries
/// (Degree, Masters, Certifications, Awards, Innovation, Training) with optional
/// supporting-document attachments. Marks are capped per company policy (default: 10,
/// matching the Achievement Score component's 10% weighting) via GradeBands-style config.
/// Only the owning HOD may add/edit/delete, and only while their HODEvaluation is Draft.
/// </summary>
public class AchievementService : IAchievementService
{
    private const decimal MaxTotalAchievementMarks = 10m;

    private readonly IEmployeeAchievementRepository _employeeAchievementRepository;
    private readonly IHODEvaluationRepository _hodEvaluationRepository;
    private readonly IAchievementTypeRepository _achievementTypeRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public AchievementService(
        IEmployeeAchievementRepository employeeAchievementRepository,
        IHODEvaluationRepository hodEvaluationRepository,
        IAchievementTypeRepository achievementTypeRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _employeeAchievementRepository = employeeAchievementRepository;
        _hodEvaluationRepository = hodEvaluationRepository;
        _achievementTypeRepository = achievementTypeRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<List<EmployeeAchievementDto>> GetByHodEvaluationAsync(int callerId, string callerRole, int hodEvaluationId)
    {
        var hodEvaluation = await _hodEvaluationRepository.GetByIdAsync(hodEvaluationId)
            ?? throw new NotFoundException("HOD Evaluation", hodEvaluationId);

        var allowed = callerRole == RoleNames.Admin || hodEvaluation.HODId == callerId;
        if (!allowed)
        {
            throw new ForbiddenException("You are not authorized to view these achievements.");
        }

        var achievements = await _employeeAchievementRepository.GetByHodEvaluationAsync(hodEvaluationId);
        return _mapper.Map<List<EmployeeAchievementDto>>(achievements);
    }

    public async Task<EmployeeAchievementDto> CreateAsync(int hodId, CreateEmployeeAchievementDto request)
    {
        await ValidateHodOwnsEvaluationAsync(hodId, request.HODEvaluationId);

        if (await _achievementTypeRepository.GetByIdAsync(request.AchievementTypeId) is null)
        {
            throw new NotFoundException("Achievement Type", request.AchievementTypeId);
        }

        await ValidateMarksCapAsync(request.HODEvaluationId, request.Marks);

        var achievement = new EmployeeAchievement
        {
            HODEvaluationId = request.HODEvaluationId,
            AchievementTypeId = request.AchievementTypeId,
            Title = request.Title,
            Marks = request.Marks,
            Remarks = request.Remarks
        };

        await _employeeAchievementRepository.AddAsync(achievement);
        await LogAuditAsync(hodId, "Create", "EmployeeAchievements", achievement.AchievementId);
        await _employeeAchievementRepository.SaveChangesAsync();

        var created = await _employeeAchievementRepository.GetByIdAsync(achievement.AchievementId);
        return _mapper.Map<EmployeeAchievementDto>(created!);
    }

    public async Task<EmployeeAchievementDto> UpdateAsync(int hodId, int achievementId, UpdateEmployeeAchievementDto request)
    {
        var achievement = await _employeeAchievementRepository.GetByIdAsync(achievementId)
            ?? throw new NotFoundException("Employee Achievement", achievementId);

        await ValidateHodOwnsEvaluationAsync(hodId, achievement.HODEvaluationId);

        if (await _achievementTypeRepository.GetByIdAsync(request.AchievementTypeId) is null)
        {
            throw new NotFoundException("Achievement Type", request.AchievementTypeId);
        }

        await ValidateMarksCapAsync(achievement.HODEvaluationId, request.Marks, excludeAchievementId: achievementId);

        achievement.AchievementTypeId = request.AchievementTypeId;
        achievement.Title = request.Title;
        achievement.Marks = request.Marks;
        achievement.Remarks = request.Remarks;

        await _employeeAchievementRepository.UpdateAsync(achievement);
        await LogAuditAsync(hodId, "Update", "EmployeeAchievements", achievementId);
        await _employeeAchievementRepository.SaveChangesAsync();

        return _mapper.Map<EmployeeAchievementDto>(achievement);
    }

    public async Task DeleteAsync(int hodId, int achievementId)
    {
        var achievement = await _employeeAchievementRepository.GetByIdAsync(achievementId)
            ?? throw new NotFoundException("Employee Achievement", achievementId);

        await ValidateHodOwnsEvaluationAsync(hodId, achievement.HODEvaluationId);

        await _employeeAchievementRepository.DeleteAsync(achievement);
        await LogAuditAsync(hodId, "Delete", "EmployeeAchievements", achievementId);
        await _employeeAchievementRepository.SaveChangesAsync();
    }

    public async Task<EmployeeAchievementDto> UploadAttachmentAsync(int hodId, int achievementId, string storedFilePath)
    {
        var achievement = await _employeeAchievementRepository.GetByIdAsync(achievementId)
            ?? throw new NotFoundException("Employee Achievement", achievementId);

        await ValidateHodOwnsEvaluationAsync(hodId, achievement.HODEvaluationId);

        achievement.Attachment = storedFilePath;
        await _employeeAchievementRepository.UpdateAsync(achievement);
        await LogAuditAsync(hodId, "Update", "EmployeeAchievements", achievementId);
        await _employeeAchievementRepository.SaveChangesAsync();

        return _mapper.Map<EmployeeAchievementDto>(achievement);
    }

    // ---------- private helpers ----------

    private async Task ValidateHodOwnsEvaluationAsync(int hodId, int hodEvaluationId)
    {
        var hodEvaluation = await _hodEvaluationRepository.GetByIdAsync(hodEvaluationId)
            ?? throw new NotFoundException("HOD Evaluation", hodEvaluationId);

        if (hodEvaluation.HODId != hodId)
        {
            throw new ForbiddenException("You can only manage achievements on your own HOD evaluation.");
        }

        if (hodEvaluation.Status != "Draft")
        {
            throw new ConflictException("Achievements can only be managed while the HOD evaluation is still in Draft.");
        }
    }

    private async Task ValidateMarksCapAsync(int hodEvaluationId, decimal newMarks, int? excludeAchievementId = null)
    {
        var currentTotal = await _employeeAchievementRepository.GetTotalMarksAsync(hodEvaluationId, excludeAchievementId);
        if (currentTotal + newMarks > MaxTotalAchievementMarks)
        {
            throw new ValidationAppException(
                $"Total Special Achievement marks cannot exceed {MaxTotalAchievementMarks}. " +
                $"Current total is {currentTotal}, and this entry would bring it to {currentTotal + newMarks}.");
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
