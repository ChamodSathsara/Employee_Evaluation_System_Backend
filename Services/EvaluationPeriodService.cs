using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Admin-only CRUD and activation for evaluation periods. Exactly one period may be Active
/// at a time — activating a period automatically deactivates any previously-Active period.
/// KPI creation and evaluations are only permitted against the Active period (see
/// KpiAssignmentService.CreateAsync).
/// </summary>
public class EvaluationPeriodService : IEvaluationPeriodService
{
    private readonly IEvaluationPeriodRepository _evaluationPeriodRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public EvaluationPeriodService(
        IEvaluationPeriodRepository evaluationPeriodRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _evaluationPeriodRepository = evaluationPeriodRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<EvaluationPeriodDto>> GetAllAsync()
    {
        var periods = await _evaluationPeriodRepository.GetAllOrderedAsync();
        return _mapper.Map<List<EvaluationPeriodDto>>(periods);
    }

    public async Task<EvaluationPeriodDto> GetByIdAsync(int periodId)
    {
        var period = await _evaluationPeriodRepository.GetByIdAsync(periodId)
            ?? throw new NotFoundException("Evaluation Period", periodId);

        return _mapper.Map<EvaluationPeriodDto>(period);
    }

    public async Task<EvaluationPeriodDto> CreateAsync(CreateEvaluationPeriodDto request)
    {
        EnsureCallerIsAdmin();
        ValidateDateRange(request.StartDate, request.EndDate);

        if (await _evaluationPeriodRepository.ExistsByNameAsync(request.Name))
        {
            throw new ConflictException($"An evaluation period named '{request.Name}' already exists.");
        }

        var period = _mapper.Map<EvaluationPeriod>(request);
        period.Status = "Inactive";

        await _evaluationPeriodRepository.AddAsync(period);
        await LogAuditAsync("Create", "EvaluationPeriods", period.PeriodId);
        await _evaluationPeriodRepository.SaveChangesAsync();

        return _mapper.Map<EvaluationPeriodDto>(period);
    }

    public async Task<EvaluationPeriodDto> UpdateAsync(int periodId, UpdateEvaluationPeriodDto request)
    {
        EnsureCallerIsAdmin();
        ValidateDateRange(request.StartDate, request.EndDate);

        var period = await _evaluationPeriodRepository.GetByIdAsync(periodId)
            ?? throw new NotFoundException("Evaluation Period", periodId);

        if (await _evaluationPeriodRepository.ExistsByNameAsync(request.Name, excludePeriodId: periodId))
        {
            throw new ConflictException($"An evaluation period named '{request.Name}' already exists.");
        }

        _mapper.Map(request, period);
        await _evaluationPeriodRepository.UpdateAsync(period);
        await LogAuditAsync("Update", "EvaluationPeriods", periodId);
        await _evaluationPeriodRepository.SaveChangesAsync();

        return _mapper.Map<EvaluationPeriodDto>(period);
    }

    public async Task<EvaluationPeriodDto> ActivateAsync(int periodId)
    {
        EnsureCallerIsAdmin();

        var period = await _evaluationPeriodRepository.GetByIdAsync(periodId)
            ?? throw new NotFoundException("Evaluation Period", periodId);

        await _evaluationPeriodRepository.DeactivateAllExceptAsync(periodId);
        period.Status = "Active";

        await _evaluationPeriodRepository.UpdateAsync(period);
        await LogAuditAsync("Update", "EvaluationPeriods", periodId);
        await _evaluationPeriodRepository.SaveChangesAsync();

        return _mapper.Map<EvaluationPeriodDto>(period);
    }

    public async Task DeleteAsync(int periodId)
    {
        EnsureCallerIsAdmin();

        var period = await _evaluationPeriodRepository.GetByIdAsync(periodId)
            ?? throw new NotFoundException("Evaluation Period", periodId);

        if (await _evaluationPeriodRepository.HasKpiAssignmentsAsync(periodId))
        {
            throw new ConflictException("This evaluation period has KPI assignments and cannot be deleted.");
        }

        await _evaluationPeriodRepository.DeleteAsync(period);
        await LogAuditAsync("Delete", "EvaluationPeriods", periodId);
        await _evaluationPeriodRepository.SaveChangesAsync();
    }

    private static void ValidateDateRange(DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
        {
            throw new ValidationAppException("End date must be after start date.");
        }
    }

    private void EnsureCallerIsAdmin()
    {
        if (!_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only Admin can manage evaluation periods.");
        }
    }

    private async Task LogAuditAsync(string action, string tableName, int? recordId)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = _currentUserService.UserId ?? 0,
            Action = action,
            TableName = tableName,
            RecordId = recordId,
            CreatedDate = DateTime.UtcNow
        });
    }
}
