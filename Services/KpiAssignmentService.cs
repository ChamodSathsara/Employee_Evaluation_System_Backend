using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements KPI Assignment creation, editing, publishing, and retrieval, enforcing the
/// role/hierarchy creation matrix, the "creator + Draft only" edit rule, and the
/// "weights must total exactly 100%" rule (SRS Module 1 / Step 2-5 of the business workflow).
/// </summary>
public class KpiAssignmentService : IKpiAssignmentService
{
    private const string StatusDraft = "Draft";
    private const string StatusPublished = "Published";

    private readonly IKPIAssignmentRepository _kpiAssignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEvaluationPeriodRepository _evaluationPeriodRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public KpiAssignmentService(
        IKPIAssignmentRepository kpiAssignmentRepository,
        IUserRepository userRepository,
        IEvaluationPeriodRepository evaluationPeriodRepository,
        INotificationRepository notificationRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _kpiAssignmentRepository = kpiAssignmentRepository;
        _userRepository = userRepository;
        _evaluationPeriodRepository = evaluationPeriodRepository;
        _notificationRepository = notificationRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<KpiAssignmentDto> CreateAsync(int callerId, CreateKpiAssignmentDto request)
    {
        var creator = await _userRepository.GetByIdWithDetailsAsync(callerId)
            ?? throw new NotFoundException("User", callerId);

        var employee = await _userRepository.GetByIdWithDetailsAsync(request.EmployeeId)
            ?? throw new NotFoundException("Employee", request.EmployeeId);

        ValidateCreatorTargetRolePair(creator.Role.RoleName, employee.Role.RoleName);
        ValidateDirectReport(creator, employee);

        var period = await _evaluationPeriodRepository.GetByIdAsync(request.PeriodId)
            ?? throw new NotFoundException("Evaluation Period", request.PeriodId);

        if (!string.Equals(period.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationAppException($"Evaluation period '{period.Name}' is not active. KPIs can only be created for an active period.");
        }

        var existing = await _kpiAssignmentRepository.GetByEmployeeAndPeriodAsync(request.EmployeeId, request.PeriodId);
        if (existing is not null)
        {
            throw new ConflictException($"A KPI assignment already exists for this employee in period '{period.Name}'.");
        }

        ValidateTotalWeight(request.Details);

        var assignment = new KPIAssignment
        {
            EmployeeId = request.EmployeeId,
            CreatedBy = callerId,
            PeriodId = request.PeriodId,
            Status = StatusDraft,
            CreatedDate = DateTime.UtcNow,
            Details = request.Details.Select(d => new KPIAssignmentDetail
            {
                KPIName = d.KPIName,
                Description = d.Description,
                WeightPercentage = d.WeightPercentage
            }).ToList()
        };

        await _kpiAssignmentRepository.AddAsync(assignment);
        await LogAuditAsync(callerId, "Create", "KPIAssignments", assignment.AssignmentId);
        await _kpiAssignmentRepository.SaveChangesAsync();

        var created = await _kpiAssignmentRepository.GetByIdWithDetailsAsync(assignment.AssignmentId);
        return _mapper.Map<KpiAssignmentDto>(created!);
    }

    public async Task<KpiAssignmentDto> UpdateAsync(int callerId, int assignmentId, UpdateKpiAssignmentDto request)
    {
        var assignment = await _kpiAssignmentRepository.GetByIdWithDetailsAsync(assignmentId)
            ?? throw new NotFoundException("KPI Assignment", assignmentId);

        EnsureCreatorAndDraft(assignment, callerId, "edit");

        ValidateTotalWeight(request.Details);

        var newDetails = request.Details.Select(d => new KPIAssignmentDetail
        {
            KPIName = d.KPIName,
            Description = d.Description,
            WeightPercentage = d.WeightPercentage
        }).ToList();

        await _kpiAssignmentRepository.ReplaceDetailsAsync(assignmentId, newDetails);
        await LogAuditAsync(callerId, "Update", "KPIAssignments", assignmentId);
        await _kpiAssignmentRepository.SaveChangesAsync();

        var updated = await _kpiAssignmentRepository.GetByIdWithDetailsAsync(assignmentId);
        return _mapper.Map<KpiAssignmentDto>(updated!);
    }

    public async Task DeleteAsync(int callerId, int assignmentId)
    {
        var assignment = await _kpiAssignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("KPI Assignment", assignmentId);

        EnsureCreatorAndDraft(assignment, callerId, "delete");

        if (await _kpiAssignmentRepository.HasEvaluationsAsync(assignmentId))
        {
            throw new ConflictException("This KPI assignment already has evaluation activity and cannot be deleted.");
        }

        await _kpiAssignmentRepository.DeleteAsync(assignment);
        await LogAuditAsync(callerId, "Delete", "KPIAssignments", assignmentId);
        await _kpiAssignmentRepository.SaveChangesAsync();
    }

    public async Task<KpiAssignmentDto> PublishAsync(int callerId, int assignmentId)
    {
        var assignment = await _kpiAssignmentRepository.GetByIdWithDetailsAsync(assignmentId)
            ?? throw new NotFoundException("KPI Assignment", assignmentId);

        EnsureCreatorAndDraft(assignment, callerId, "publish");

        var totalWeight = assignment.Details.Sum(d => d.WeightPercentage);
        if (totalWeight != 100)
        {
            throw new ValidationAppException($"Total KPI weight must equal exactly 100%. Current total is {totalWeight}%.");
        }

        assignment.Status = StatusPublished;
        await _kpiAssignmentRepository.UpdateAsync(assignment);
        await LogAuditAsync(callerId, "Publish", "KPIAssignments", assignmentId);

        await _notificationRepository.AddAsync(new Notification
        {
            UserId = assignment.EmployeeId,
            Title = "New KPIs Published",
            Message = $"Your KPIs for period '{assignment.Period.Name}' have been published. You may now begin your self-evaluation.",
            IsRead = false
        });

        await _kpiAssignmentRepository.SaveChangesAsync();

        return _mapper.Map<KpiAssignmentDto>(assignment);
    }

    public async Task<KpiAssignmentDto> RevertToDraftAsync(int callerId, int assignmentId)
    {
        var assignment = await _kpiAssignmentRepository.GetByIdWithDetailsAsync(assignmentId)
            ?? throw new NotFoundException("KPI Assignment", assignmentId);

        if (assignment.CreatedBy != callerId)
        {
            throw new ForbiddenException("Only the creator of this KPI assignment can revert it to Draft.");
        }

        if (assignment.Status != StatusPublished)
        {
            throw new ValidationAppException("Only a Published KPI assignment can be reverted to Draft.");
        }

        if (await _kpiAssignmentRepository.HasEvaluationsAsync(assignmentId))
        {
            throw new ConflictException("This KPI assignment already has evaluation activity and can no longer be reverted to Draft.");
        }

        assignment.Status = StatusDraft;
        await _kpiAssignmentRepository.UpdateAsync(assignment);
        await LogAuditAsync(callerId, "Update", "KPIAssignments", assignmentId);

        await _notificationRepository.AddAsync(new Notification
        {
            UserId = assignment.EmployeeId,
            Title = "KPIs Reopened for Correction",
            Message = $"Your KPIs for period '{assignment.Period.Name}' have been reverted to Draft by the creator for correction.",
            IsRead = false
        });

        await _kpiAssignmentRepository.SaveChangesAsync();

        return _mapper.Map<KpiAssignmentDto>(assignment);
    }

    public async Task<List<KpiAssignmentDto>> GetMyKpisAsync(int callerId)
    {
        var assignments = await _kpiAssignmentRepository.GetMyPublishedAsync(callerId);
        return _mapper.Map<List<KpiAssignmentDto>>(assignments);
    }

    public async Task<KpiAssignmentDto> GetByIdAsync(int callerId, string callerRole, int assignmentId)
    {
        var assignment = await _kpiAssignmentRepository.GetByIdWithDetailsAsync(assignmentId)
            ?? throw new NotFoundException("KPI Assignment", assignmentId);

        var allowed = callerRole == RoleNames.Admin
            || assignment.EmployeeId == callerId
            || assignment.CreatedBy == callerId
            || assignment.Employee.ReportsTo == callerId;

        if (!allowed)
        {
            throw new ForbiddenException("You are not authorized to view this KPI assignment.");
        }

        return _mapper.Map<KpiAssignmentDto>(assignment);
    }

    public async Task<List<KpiAssignmentDto>> GetByEmployeeAsync(int callerId, string callerRole, int employeeId)
    {
        if (await _userRepository.GetByIdAsync(employeeId) is null)
        {
            throw new NotFoundException("Employee", employeeId);
        }

        //if (callerRole != RoleNames.Admin && callerId != employeeId || callerRole != RoleNames.Manager)
        //{
        //    var subordinateIds = await GetSubordinateUserIdsAsync(callerId);
        //    if (!subordinateIds.Contains(employeeId))
        //    {
        //        throw new ForbiddenException("You are not authorized to view this employee's KPI assignments.");
        //    }
        //}

        var assignments = await _kpiAssignmentRepository.GetByEmployeeAsync(employeeId);
        return _mapper.Map<List<KpiAssignmentDto>>(assignments);
    }

    public async Task<List<KpiAssignmentDto>> GetByPeriodAsync(int callerId, string callerRole, int periodId)
    {
        if (await _evaluationPeriodRepository.GetByIdAsync(periodId) is null)
        {
            throw new NotFoundException("Evaluation Period", periodId);
        }

        // Admin sees every assignment in the period. Every other role is scoped to their
        // own organizational subtree (computed by walking the ReportsTo chain downward).
        IReadOnlySet<int>? scope = null;
        if (callerRole != RoleNames.Admin)
        {
            scope = await GetSubordinateUserIdsAsync(callerId);
        }

        var assignments = await _kpiAssignmentRepository.GetByPeriodAsync(periodId, scope);
        return _mapper.Map<List<KpiAssignmentDto>>(assignments);
    }

    public async Task<List<KpiAssignmentDto>> GetMyTeamKpisAsync(int callerId)
    {
        var directReports = await _userRepository.GetDirectReportsAsync(callerId);

        if (directReports.Count == 0)
        {
            return new List<KpiAssignmentDto>();
        }

        var employeeIds = directReports.Select(u => u.UserId);
        var assignments = await _kpiAssignmentRepository.GetByEmployeeIdsAsync(employeeIds);

        return _mapper.Map<List<KpiAssignmentDto>>(assignments);
    }

    // ---------- private helpers ----------

    /// <summary>Validates the fixed creator-role -> target-role matrix from the business workflow.</summary>
    private static void ValidateCreatorTargetRolePair(string creatorRole, string targetRole)
    {
        var isValid = creatorRole switch
        {
            RoleNames.CEO => targetRole is RoleNames.Manager or RoleNames.HOD,
            RoleNames.Manager => targetRole == RoleNames.HOD,
            RoleNames.HOD => targetRole is RoleNames.Supervisor or RoleNames.Employee,
            RoleNames.Supervisor => targetRole == RoleNames.Employee,
            _ => false
        };

        if (!isValid)
        {
            throw new ForbiddenException(
                $"A user with role '{creatorRole}' is not permitted to create a KPI assignment for a user with role '{targetRole}'.");
        }
    }

    /// <summary>Validates the target employee directly reports to the creator (ReportsTo == creator.UserId).</summary>
    private static void ValidateDirectReport(User creator, User employee)
    {
        if (employee.ReportsTo != creator.UserId)
        {
            throw new ForbiddenException(
                $"'{employee.FirstName} {employee.LastName}' does not directly report to you. KPIs may only be assigned to your direct reports.");
        }
    }

    private static void ValidateTotalWeight(List<CreateKpiAssignmentDetailDto> details)
    {
        if (details is null || details.Count == 0)
        {
            throw new ValidationAppException("At least one KPI line item is required.");
        }

        var total = details.Sum(d => d.WeightPercentage);
        if (total != 100)
        {
            throw new ValidationAppException($"Total KPI weight must equal exactly 100%. You submitted {total}%.");
        }
    }

    private static void EnsureCreatorAndDraft(KPIAssignment assignment, int callerId, string action)
    {
        if (assignment.CreatedBy != callerId)
        {
            throw new ForbiddenException($"Only the creator of this KPI assignment can {action} it.");
        }

        if (assignment.Status != StatusDraft)
        {
            throw new ConflictException("Published KPIs cannot be edited. Revert the assignment to Draft first if changes are required.");
        }
    }

    /// <summary>
    /// Walks the ReportsTo chain downward from <paramref name="rootUserId"/> via
    /// breadth-first traversal of direct reports, returning every user reachable in the
    /// caller's organizational subtree (their direct/indirect reports). Used to scope
    /// visibility for non-Admin roles across the KPI Assignment module.
    /// </summary>
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