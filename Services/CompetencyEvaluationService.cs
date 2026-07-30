using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements Competency Evaluation (SRS Step 9): the HOD scores the employee against each
/// of the 10 standard competencies on a 1-5 scale. Only the owning HOD may score, and only
/// while their HODEvaluation is still Draft (i.e. before submission). One score per
/// competency per HOD evaluation — submitting again for the same competency updates it.
/// </summary>
public class CompetencyEvaluationService : ICompetencyEvaluationService
{
    private readonly ICompetencyEvaluationRepository _competencyEvaluationRepository;
    private readonly IHODEvaluationRepository _hodEvaluationRepository;
    private readonly ICompetencyRepository _competencyRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public CompetencyEvaluationService(
        ICompetencyEvaluationRepository competencyEvaluationRepository,
        IHODEvaluationRepository hodEvaluationRepository,
        ICompetencyRepository competencyRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _competencyEvaluationRepository = competencyEvaluationRepository;
        _hodEvaluationRepository = hodEvaluationRepository;
        _competencyRepository = competencyRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<List<CompetencyEvaluationDto>> GetByHodEvaluationAsync(int callerId, string callerRole, int hodEvaluationId)
    {
        var hodEvaluation = await _hodEvaluationRepository.GetByIdAsync(hodEvaluationId)
            ?? throw new NotFoundException("HOD Evaluation", hodEvaluationId);

        var allowed = callerRole == RoleNames.Admin || hodEvaluation.HODId == callerId;
        if (!allowed)
        {
            throw new ForbiddenException("You are not authorized to view these competency scores.");
        }

        var scores = await _competencyEvaluationRepository.GetByHodEvaluationAsync(hodEvaluationId);
        return _mapper.Map<List<CompetencyEvaluationDto>>(scores);
    }

    public async Task<CompetencyEvaluationDto> SubmitAsync(int hodId, int hodEvaluationId, SubmitCompetencyEvaluationDto request)
    {
        var hodEvaluation = await ValidateHodOwnsEvaluationAsync(hodId, hodEvaluationId);

        if (await _competencyRepository.GetByIdAsync(request.CompetencyId) is null)
        {
            throw new NotFoundException("Competency", request.CompetencyId);
        }

        var existing = await _competencyEvaluationRepository.GetByHodEvaluationAndCompetencyAsync(hodEvaluationId, request.CompetencyId);

        if (existing is not null)
        {
            existing.Score = request.Score;
            existing.Remarks = request.Remarks;
            await _competencyEvaluationRepository.UpdateAsync(existing);
            await LogAuditAsync(hodId, "Update", "CompetencyEvaluations", existing.CompetencyEvaluationId);
            await _competencyEvaluationRepository.SaveChangesAsync();

            var updated = await _competencyEvaluationRepository.GetByHodEvaluationAndCompetencyAsync(hodEvaluationId, request.CompetencyId);
            return _mapper.Map<CompetencyEvaluationDto>(updated!);
        }

        var scoreEntry = new CompetencyEvaluation
        {
            HODEvaluationId = hodEvaluationId,
            CompetencyId = request.CompetencyId,
            Score = request.Score,
            Remarks = request.Remarks
        };

        await _competencyEvaluationRepository.AddAsync(scoreEntry);
        await LogAuditAsync(hodId, "Create", "CompetencyEvaluations", scoreEntry.CompetencyEvaluationId);
        await _competencyEvaluationRepository.SaveChangesAsync();

        var created = await _competencyEvaluationRepository.GetByHodEvaluationAndCompetencyAsync(hodEvaluationId, request.CompetencyId);
        return _mapper.Map<CompetencyEvaluationDto>(created!);
    }

    public async Task<CompetencyEvaluationDto> UpdateAsync(int hodId, int competencyEvaluationId, UpdateCompetencyEvaluationDto request)
    {
        var scoreEntry = await _competencyEvaluationRepository.GetByIdAsync(competencyEvaluationId)
            ?? throw new NotFoundException("Competency Evaluation", competencyEvaluationId);

        await ValidateHodOwnsEvaluationAsync(hodId, scoreEntry.HODEvaluationId);

        scoreEntry.Score = request.Score;
        scoreEntry.Remarks = request.Remarks;

        await _competencyEvaluationRepository.UpdateAsync(scoreEntry);
        await LogAuditAsync(hodId, "Update", "CompetencyEvaluations", competencyEvaluationId);
        await _competencyEvaluationRepository.SaveChangesAsync();

        return _mapper.Map<CompetencyEvaluationDto>(scoreEntry);
    }

    // ---------- private helpers ----------

    private async Task<HODEvaluation> ValidateHodOwnsEvaluationAsync(int hodId, int hodEvaluationId)
    {
        var hodEvaluation = await _hodEvaluationRepository.GetByIdAsync(hodEvaluationId)
            ?? throw new NotFoundException("HOD Evaluation", hodEvaluationId);

        if (hodEvaluation.HODId != hodId)
        {
            throw new ForbiddenException("You can only score competencies on your own HOD evaluation.");
        }

        if (hodEvaluation.Status != "Draft")
        {
            throw new ConflictException("Competency scores can only be entered while the HOD evaluation is still in Draft.");
        }

        return hodEvaluation;
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
