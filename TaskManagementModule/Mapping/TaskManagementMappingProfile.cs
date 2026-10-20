using AutoMapper;
using TaskManagementModule.Data.Domain;

namespace TaskManagementModule.Mapping;

public sealed class TaskManagementMappingProfile : Profile
{
    public TaskManagementMappingProfile()
    {
        CreateMap<Employee, Models.Employee>();
        CreateMap<WorkTask, Models.WorkTask>();
    }
}
