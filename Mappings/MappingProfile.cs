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
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartmentName))
            .ForMember(dest => dest.DesignationName, opt => opt.MapFrom(src => src.Designation.DesignationName));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.DepartmentName))
            .ForMember(dest => dest.DesignationName, opt => opt.MapFrom(src => src.Designation.DesignationName))
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
    }
}
