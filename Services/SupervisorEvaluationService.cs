using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements Supervisor Review (SRS Step 7, optional layer): the Supervisor views the
/// employee's self-evaluation marks, enters their own marks/comments per KPI, and submits.
/// Only the employee's direct Supervisor (ReportsTo) may review; Draft may be edited freely,
/// Submitted locks the record.
/// </summary>
public class SupervisorEvaluationService : ISupervisorEvaluationService
{
    private const string StatusDraft = "Draft";
    private const string StatusSubmitted = "Submitted";

    private readonly ISupervisorEvaluationRepository _supervisorEvaluationRepository;
    private readonly IEmployeeEvaluationRepository _employeeEvaluationRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public SupervisorEvaluationService(
        ISupervisorEvaluationRepository supervisorEvaluationRepository,
        IEmployeeEvaluationRepository employeeEvaluationRepository,
        INotificationRepository notificationRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _supervisorEvaluationRepository = supervisorEvaluationRepository;
        _employeeEvaluationRepository = employeeEvaluationRepository;
        _notificationRepository = notificationRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<List<PendingReviewDto>> GetPendingAsync(int supervisorId)
    {
        var pending = await _employeeEvaluationRepository.GetPendingForSupervisorAsync(supervisorId);

        return pending.Select(e => new PendingReviewDto
        {
            SelfEvaluationId = e.SelfEvaluationId,
            AssignmentId = e.AssignmentId,
            EmployeeId = e.EmployeeId,
            EmployeeName = $"{e.Employee.FirstName} {e.Employee.LastName}",
            PeriodName = e.Assignment.Period.Name,
            SubmittedDate = e.SubmittedDate
        }).ToList();
    }

    public async Task<SupervisorEvaluationDto> GetByIdAsync(int callerId, string callerRole, int supervisorEvaluationId)
    {
        var evaluation = await _supervisorEvaluationRepository.GetByIdWithDetailsAsync(supervisorEvaluationId)
            ?? throw new NotFoundException("Supervisor Evaluation", supervisorEvaluationId);

        var allowed = callerRole == RoleNames.Admin
            || evaluation.SupervisorId == callerId
            || evaluation.SelfEvaluation.EmployeeId == callerId;

        if (!allowed)
        {
            throw new ForbiddenException("You are not authorized to view this supervisor evaluation.");
        }

        return MapWithEmployeeMarks(evaluation);
    }

    public async Task<SupervisorEvaluationDto> CreateAsync(int supervisorId, CreateSupervisorEvaluationDto request)
    {
        var selfEvaluation = await _employeeEvaluationRepository.GetByIdWithDetailsAsync(request.SelfEvaluationId)
            ?? throw new NotFoundException("Employee Evaluation", request.SelfEvaluationId);

        ValidateSupervisorOwnsEmployee(supervisorId, selfEvaluation);

        if (selfEvaluation.Status != StatusSubmitted)
        {
            throw new ValidationAppException("The employee's self-evaluation must be Submitted before Supervisor review can begin.");
        }

        var existing = await _supervisorEvaluationRepository.GetBySelfEvaluationAsync(request.SelfEvaluationId);
        if (existing is not null)
        {
            throw new ConflictException("A Supervisor evaluation already exists for this self-evaluation.");
        }

        var validDetailIds = selfEvaluation.Details.Select(d => d.KPIAssignmentDetailId).ToHashSet();
        ValidateDetailsAgainstSelfEvaluation(validDetailIds, request.Details);

        var evaluation = new SupervisorEvaluation
        {
            SelfEvaluationId = request.SelfEvaluationId,
            SupervisorId = supervisorId,
            Status = StatusDraft,
            Details = request.Details.Select(d => new SupervisorEvaluationDetail
            {
                KPIAssignmentDetailId = d.KPIAssignmentDetailId,
                Marks = d.Marks,
                Comments = d.Comments
            }).ToList()
        };

        await _supervisorEvaluationRepository.AddAsync(evaluation);
        await LogAuditAsync(supervisorId, "Create", "SupervisorEvaluations", evaluation.SupervisorEvaluationId);
        await _supervisorEvaluationRepository.SaveChangesAsync();

        var created = await _supervisorEvaluationRepository.GetByIdWithDetailsAsync(evaluation.SupervisorEvaluationId);
        return MapWithEmployeeMarks(created!);
    }

    public async Task<SupervisorEvaluationDto> UpdateAsync(int supervisorId, int supervisorEvaluationId, UpdateSupervisorEvaluationDto request)
    {
        var evaluation = await _supervisorEvaluationRepository.GetByIdWithDetailsAsync(supervisorEvaluationId)
            ?? throw new NotFoundException("Supervisor Evaluation", supervisorEvaluationId);

        if (evaluation.SupervisorId != supervisorId)
        {
            throw new ForbiddenException("You can only edit your own supervisor evaluation.");
        }

        if (evaluation.Status != StatusDraft)
        {
            throw new ConflictException("This supervisor evaluation has already been submitted and can no longer be edited.");
        }

        var validDetailIds = evaluation.SelfEvaluation.Details.Select(d => d.KPIAssignmentDetailId).ToHashSet();
        ValidateDetailsAgainstSelfEvaluation(validDetailIds, request.Details);

        var newDetails = request.Details.Select(d => new SupervisorEvaluationDetail
        {
            KPIAssignmentDetailId = d.KPIAssignmentDetailId,
            Marks = d.Marks,
            Comments = d.Comments
        }).ToList();

        await _supervisorEvaluationRepository.ReplaceDetailsAsync(supervisorEvaluationId, newDetails);
        await LogAuditAsync(supervisorId, "Update", "SupervisorEvaluations", supervisorEvaluationId);
        await _supervisorEvaluationRepository.SaveChangesAsync();

        var updated = await _supervisorEvaluationRepository.GetByIdWithDetailsAsync(supervisorEvaluationId);
        return MapWithEmployeeMarks(updated!);
    }

    public async Task<SupervisorEvaluationDto> SubmitAsync(int supervisorId, int supervisorEvaluationId)
    {
        var evaluation = await _supervisorEvaluationRepository.GetByIdWithDetailsAsync(supervisorEvaluationId)
            ?? throw new NotFoundException("Supervisor Evaluation", supervisorEvaluationId);

        if (evaluation.SupervisorId != supervisorId)
        {
            throw new ForbiddenException("You can only submit your own supervisor evaluation.");
        }

        if (evaluation.Status != StatusDraft)
        {
            throw new ConflictException("This supervisor evaluation has already been submitted.");
        }

        evaluation.Status = StatusSubmitted;
        evaluation.SubmittedDate = DateTime.UtcNow;

        await _supervisorEvaluationRepository.UpdateAsync(evaluation);
        await LogAuditAsync(supervisorId, "Evaluation Submit", "SupervisorEvaluations", supervisorEvaluationId);
        await _supervisorEvaluationRepository.SaveChangesAsync();

        var result = await _supervisorEvaluationRepository.GetByIdWithDetailsAsync(supervisorEvaluationId);
        return MapWithEmployeeMarks(result!);
    }

    // ---------- private helpers ----------

    private static void ValidateSupervisorOwnsEmployee(int supervisorId, EmployeeEvaluation selfEvaluation)
    {
        if (selfEvaluation.Employee.ReportedSupervisorId != supervisorId)
        {
            throw new ForbiddenException("This employee does not directly report to you.");
        }
    }

    private static void ValidateDetailsAgainstSelfEvaluation(HashSet<int> validDetailIds, List<SupervisorEvaluationDetailInputDto> submittedDetails)
    {
        foreach (var detail in submittedDetails)
        {
            if (!validDetailIds.Contains(detail.KPIAssignmentDetailId))
            {
                throw new ValidationAppException($"KPI line item {detail.KPIAssignmentDetailId} does not belong to this evaluation.");
            }
        }

        if (submittedDetails.Select(d => d.KPIAssignmentDetailId).Distinct().Count() != validDetailIds.Count)
        {
            throw new ValidationAppException("All KPI line items must be marked before saving.");
        }
    }

    private static SupervisorEvaluationDto MapWithEmployeeMarks(SupervisorEvaluation evaluation)
    {
        var employeeMarksByDetail = evaluation.SelfEvaluation.Details.ToDictionary(d => d.KPIAssignmentDetailId, d => d.EmployeeMarks);

        var dto = new SupervisorEvaluationDto
        {
            SupervisorEvaluationId = evaluation.SupervisorEvaluationId,
            SelfEvaluationId = evaluation.SelfEvaluationId,
            EmployeeId = evaluation.SelfEvaluation.EmployeeId,
            EmployeeName = $"{evaluation.SelfEvaluation.Employee.FirstName} {evaluation.SelfEvaluation.Employee.LastName}",
            SupervisorId = evaluation.SupervisorId,
            SupervisorName = $"{evaluation.Supervisor.FirstName} {evaluation.Supervisor.LastName}",
            SubmittedDate = evaluation.SubmittedDate,
            Status = evaluation.Status,
            Details = evaluation.Details.Select(d => new SupervisorEvaluationDetailDto
            {
                DetailId = d.DetailId,
                KPIAssignmentDetailId = d.KPIAssignmentDetailId,
                KPIName = d.KpiAssignmentDetail.KPIName,
                WeightPercentage = d.KpiAssignmentDetail.WeightPercentage,
                EmployeeMarks = employeeMarksByDetail.GetValueOrDefault(d.KPIAssignmentDetailId),
                Marks = d.Marks,
                Comments = d.Comments
            }).ToList()
        };

        return dto;
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
