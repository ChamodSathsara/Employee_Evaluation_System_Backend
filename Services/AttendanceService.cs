using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements the Attendance & Leave Summary module (SRS Step 11). Entered or imported by
/// HOD/Admin per employee per period. The attendance penalty is calculated automatically
/// using the same banding as the source Probation Evaluation Form:
///   No-Pay: 2-3 days -2, 4-5 days -5, more than 5 days -10.
///   Late Attendance: 1-10 -1, 11-20 -3, more than 20 -5.
/// </summary>
public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceSummaryRepository _attendanceSummaryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEvaluationPeriodRepository _evaluationPeriodRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public AttendanceService(
        IAttendanceSummaryRepository attendanceSummaryRepository,
        IUserRepository userRepository,
        IEvaluationPeriodRepository evaluationPeriodRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _attendanceSummaryRepository = attendanceSummaryRepository;
        _userRepository = userRepository;
        _evaluationPeriodRepository = evaluationPeriodRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<List<AttendanceSummaryDto>> GetByEmployeeAsync(int callerId, string callerRole, int employeeId)
    {
        if (callerRole != RoleNames.Admin && callerId != employeeId)
        {
            var employee = await _userRepository.GetByIdAsync(employeeId)
                ?? throw new NotFoundException("Employee", employeeId);

            var isManagerChain = employee.ReportedSupervisorId == callerId || employee.ReportedHodId == callerId;
            if (!isManagerChain)
            {
                throw new ForbiddenException("You are not authorized to view this employee's attendance.");
            }
        }

        var records = await _attendanceSummaryRepository.GetByEmployeeAsync(employeeId);
        return _mapper.Map<List<AttendanceSummaryDto>>(records);
    }

    public async Task<AttendanceSummaryDto> UpsertAsync(int callerId, UpsertAttendanceSummaryDto request)
    {
        if (await _userRepository.GetByIdAsync(request.EmployeeId) is null)
        {
            throw new NotFoundException("Employee", request.EmployeeId);
        }

        if (await _evaluationPeriodRepository.GetByIdAsync(request.PeriodId) is null)
        {
            throw new NotFoundException("Evaluation Period", request.PeriodId);
        }

        var existing = await _attendanceSummaryRepository.GetByEmployeeAndPeriodAsync(request.EmployeeId, request.PeriodId);

        if (existing is not null)
        {
            ApplyRequest(existing, request);
            await _attendanceSummaryRepository.UpdateAsync(existing);
            await LogAuditAsync(callerId, "Update", "AttendanceSummaries", existing.AttendanceId);
            await _attendanceSummaryRepository.SaveChangesAsync();

            var updated = await _attendanceSummaryRepository.GetByIdAsync(existing.AttendanceId);
            return _mapper.Map<AttendanceSummaryDto>(updated!);
        }

        var record = new AttendanceSummary { EmployeeId = request.EmployeeId, PeriodId = request.PeriodId };
        ApplyRequest(record, request);

        await _attendanceSummaryRepository.AddAsync(record);
        await LogAuditAsync(callerId, "Create", "AttendanceSummaries", record.AttendanceId);
        await _attendanceSummaryRepository.SaveChangesAsync();

        var created = await _attendanceSummaryRepository.GetByIdAsync(record.AttendanceId);
        return _mapper.Map<AttendanceSummaryDto>(created!);
    }

    public async Task<AttendanceSummaryDto> UpdateAsync(int callerId, int attendanceId, UpsertAttendanceSummaryDto request)
    {
        var record = await _attendanceSummaryRepository.GetByIdAsync(attendanceId)
            ?? throw new NotFoundException("Attendance Summary", attendanceId);

        ApplyRequest(record, request);
        await _attendanceSummaryRepository.UpdateAsync(record);
        await LogAuditAsync(callerId, "Update", "AttendanceSummaries", attendanceId);
        await _attendanceSummaryRepository.SaveChangesAsync();

        return _mapper.Map<AttendanceSummaryDto>(record);
    }

    public async Task<int> ImportAsync(int callerId, ImportAttendanceDto request)
    {
        if (await _evaluationPeriodRepository.GetByIdAsync(request.PeriodId) is null)
        {
            throw new NotFoundException("Evaluation Period", request.PeriodId);
        }

        var importedCount = 0;

        foreach (var row in request.Rows)
        {
            if (await _userRepository.GetByIdAsync(row.EmployeeId) is null)
            {
                throw new NotFoundException("Employee", row.EmployeeId);
            }

            var upsertDto = new UpsertAttendanceSummaryDto
            {
                EmployeeId = row.EmployeeId,
                PeriodId = request.PeriodId,
                AnnualLeave = row.AnnualLeave,
                CasualLeave = row.CasualLeave,
                MedicalLeave = row.MedicalLeave,
                NoPayLeave = row.NoPayLeave,
                LateAttendance = row.LateAttendance
            };

            await UpsertAsync(callerId, upsertDto);
            importedCount++;
        }

        return importedCount;
    }

    public async Task<List<AttendanceSummaryDto>> GetSummaryByPeriodAsync(int periodId)
    {
        if (await _evaluationPeriodRepository.GetByIdAsync(periodId) is null)
        {
            throw new NotFoundException("Evaluation Period", periodId);
        }

        var records = await _attendanceSummaryRepository.GetByPeriodAsync(periodId);
        return _mapper.Map<List<AttendanceSummaryDto>>(records);
    }

    // ---------- private helpers ----------

    private static void ApplyRequest(AttendanceSummary record, UpsertAttendanceSummaryDto request)
    {
        record.AnnualLeave = request.AnnualLeave;
        record.CasualLeave = request.CasualLeave;
        record.MedicalLeave = request.MedicalLeave;
        record.NoPayLeave = request.NoPayLeave;
        record.LateAttendance = request.LateAttendance;
        record.PenaltyMarks = CalculatePenalty(request.NoPayLeave, request.LateAttendance);
    }

    /// <summary>
    /// Penalty bands per the SRS/source Probation Evaluation Form:
    /// No-Pay: 2-3 days -2, 4-5 days -5, more than 5 days -10.
    /// Late Attendance: 1-10 days -1, 11-20 days -3, more than 20 days -5.
    /// </summary>
    private static decimal CalculatePenalty(decimal noPayLeave, int lateAttendance)
    {
        decimal penalty = 0;

        penalty += noPayLeave switch
        {
            >= 2 and <= 3 => 2,
            >= 4 and <= 5 => 5,
            > 5 => 10,
            _ => 0
        };

        penalty += lateAttendance switch
        {
            >= 1 and <= 10 => 1,
            >= 11 and <= 20 => 3,
            > 20 => 5,
            _ => 0
        };

        return penalty;
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
