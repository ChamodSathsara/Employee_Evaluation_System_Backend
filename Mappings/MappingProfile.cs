using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Mappings;

/// <summary>
/// Central AutoMapper profile. Every Model <-> DTO pair used across the API is
/// registered here so mapping logic never has to be duplicated in a Service.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ---------- Auth / User ----------
        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department == null ? null : src.Department.DepartmentName))
            .ForMember(dest => dest.DesignationName, opt => opt.MapFrom(src => src.Designation == null ? null : src.Designation.DesignationName));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company == null ? null : src.Company.CompanyName))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department == null ? null : src.Department.DepartmentName))
            .ForMember(dest => dest.DesignationName, opt => opt.MapFrom(src => src.Designation == null ? null : src.Designation.DesignationName))
            .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src =>
                src.Manager == null ? null : src.Manager.FirstName + " " + src.Manager.LastName));

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());

        // ---------- KPI Assignment ----------
        CreateMap<KPIAssignmentDetail, KpiAssignmentDetailDto>();
        CreateMap<CreateKpiAssignmentDetailDto, KPIAssignmentDetail>()
            .ForMember(dest => dest.DetailId, opt => opt.Ignore())
            .ForMember(dest => dest.AssignmentId, opt => opt.Ignore());

        CreateMap<KPIAssignment, KpiAssignmentDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FirstName + " " + src.Employee.LastName))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByUser.FirstName + " " + src.CreatedByUser.LastName))
            .ForMember(dest => dest.PeriodName, opt => opt.MapFrom(src => src.Period.Name))
            .ForMember(dest => dest.TotalWeight, opt => opt.MapFrom(src => src.Details.Sum(d => d.WeightPercentage)))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

        // ---------- Lookups: Roles, Departments, Designations, Evaluation Periods ----------
        CreateMap<Role, RoleDto>().ForMember(dest => dest.UserCount, opt => opt.Ignore());
        CreateMap<CreateRoleDto, Role>().ForMember(dest => dest.RoleId, opt => opt.Ignore());
        CreateMap<UpdateRoleDto, Role>().ForMember(dest => dest.RoleId, opt => opt.Ignore());

        CreateMap<Department, DepartmentDto>().ForMember(dest => dest.UserCount, opt => opt.Ignore());
        CreateMap<CreateDepartmentDto, Department>().ForMember(dest => dest.DepartmentId, opt => opt.Ignore());
        CreateMap<UpdateDepartmentDto, Department>().ForMember(dest => dest.DepartmentId, opt => opt.Ignore());

        CreateMap<Designation, DesignationDto>().ForMember(dest => dest.UserCount, opt => opt.Ignore());
        CreateMap<CreateDesignationDto, Designation>().ForMember(dest => dest.DesignationId, opt => opt.Ignore());
        CreateMap<UpdateDesignationDto, Designation>().ForMember(dest => dest.DesignationId, opt => opt.Ignore());

        CreateMap<Company, CompanyDto>().ForMember(dest => dest.UserCount, opt => opt.Ignore());

        CreateMap<EvaluationPeriod, EvaluationPeriodDto>();
        CreateMap<CreateEvaluationPeriodDto, EvaluationPeriod>()
            .ForMember(dest => dest.PeriodId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<UpdateEvaluationPeriodDto, EvaluationPeriod>()
            .ForMember(dest => dest.PeriodId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // ---------- Employee Self Evaluation ----------
        CreateMap<EmployeeEvaluationDetail, EmployeeEvaluationDetailDto>()
            .ForMember(dest => dest.KPIName, opt => opt.MapFrom(src => src.KpiAssignmentDetail.KPIName))
            .ForMember(dest => dest.WeightPercentage, opt => opt.MapFrom(src => src.KpiAssignmentDetail.WeightPercentage));

        CreateMap<EmployeeEvaluation, EmployeeEvaluationDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FirstName + " " + src.Employee.LastName))
            .ForMember(dest => dest.PeriodId, opt => opt.MapFrom(src => src.Assignment.PeriodId))
            .ForMember(dest => dest.PeriodName, opt => opt.MapFrom(src => src.Assignment.Period.Name));

        // ---------- Supervisor Evaluation ----------
        CreateMap<SupervisorEvaluationDetail, SupervisorEvaluationDetailDto>()
            .ForMember(dest => dest.KPIName, opt => opt.MapFrom(src => src.KpiAssignmentDetail.KPIName))
            .ForMember(dest => dest.WeightPercentage, opt => opt.MapFrom(src => src.KpiAssignmentDetail.WeightPercentage))
            .ForMember(dest => dest.EmployeeMarks, opt => opt.Ignore());

        CreateMap<SupervisorEvaluation, SupervisorEvaluationDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.SelfEvaluation.EmployeeId))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.SelfEvaluation.Employee.FirstName + " " + src.SelfEvaluation.Employee.LastName))
            .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src => src.Supervisor.FirstName + " " + src.Supervisor.LastName));

        // ---------- HOD Evaluation ----------
        CreateMap<HODEvaluationDetail, HodEvaluationDetailDto>()
            .ForMember(dest => dest.KPIName, opt => opt.MapFrom(src => src.KpiAssignmentDetail.KPIName))
            .ForMember(dest => dest.WeightPercentage, opt => opt.MapFrom(src => src.KpiAssignmentDetail.WeightPercentage))
            .ForMember(dest => dest.EmployeeMarks, opt => opt.Ignore())
            .ForMember(dest => dest.SupervisorMarks, opt => opt.Ignore());

        CreateMap<HODEvaluation, HodEvaluationDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.SelfEvaluation.EmployeeId))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.SelfEvaluation.Employee.FirstName + " " + src.SelfEvaluation.Employee.LastName))
            .ForMember(dest => dest.HODName, opt => opt.MapFrom(src => src.HOD.FirstName + " " + src.HOD.LastName));

        // ---------- Competencies ----------
        CreateMap<Competency, CompetencyDto>();
        CreateMap<CreateCompetencyDto, Competency>().ForMember(dest => dest.CompetencyId, opt => opt.Ignore());
        CreateMap<UpdateCompetencyDto, Competency>().ForMember(dest => dest.CompetencyId, opt => opt.Ignore());

        CreateMap<CompetencyEvaluation, CompetencyEvaluationDto>()
            .ForMember(dest => dest.CompetencyName, opt => opt.MapFrom(src => src.Competency.Name));

        // ---------- Achievements ----------
        CreateMap<AchievementType, AchievementTypeDto>();

        CreateMap<EmployeeAchievement, EmployeeAchievementDto>()
            .ForMember(dest => dest.AchievementTypeName, opt => opt.MapFrom(src => src.AchievementType.Name));

        // ---------- Attendance ----------
        CreateMap<AttendanceSummary, AttendanceSummaryDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.FirstName + " " + src.Employee.LastName));

        // ---------- Evaluation Results / Recommendations / Manager Review ----------
        CreateMap<EvaluationResult, EvaluationResultDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.HODEvaluation.SelfEvaluation.EmployeeId))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                src.HODEvaluation.SelfEvaluation.Employee.FirstName + " " + src.HODEvaluation.SelfEvaluation.Employee.LastName));

        CreateMap<Recommendation, RecommendationDto>();

        CreateMap<ManagerReview, ManagerReviewDto>()
            .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager.FirstName + " " + src.Manager.LastName))
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.Result.HODEvaluation.SelfEvaluation.EmployeeId))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                src.Result.HODEvaluation.SelfEvaluation.Employee.FirstName + " " + src.Result.HODEvaluation.SelfEvaluation.Employee.LastName));

        // ---------- Notifications / Audit Logs ----------
        CreateMap<Notification, NotificationDto>();

        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));
    }
}
