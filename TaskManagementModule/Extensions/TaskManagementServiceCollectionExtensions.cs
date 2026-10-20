using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementModule.Data;
using TaskManagementModule.Mapping;
using TaskManagementModule.Services;

namespace TaskManagementModule.Extensions;

public static class TaskManagementServiceCollectionExtensions
{
    public static IServiceCollection AddTaskManagementServices(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<TaskManagementDbContext>(options => options.UseNpgsql(connectionString));
        services.AddAutoMapper(_ => { }, typeof(TaskManagementMappingProfile).Assembly);
        services.AddScoped<ITaskManagementService, TaskManagementService>();
        return services;
    }
}
