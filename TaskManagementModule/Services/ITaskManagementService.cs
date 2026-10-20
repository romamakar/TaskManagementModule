using TaskManagementModule.Data.Domain;
using TaskManagementModule.DTO;

namespace TaskManagementModule.Services;

public interface ITaskManagementService
{
    Task<Models.WorkTask> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<Models.WorkTask> ChangeStatusAsync(long taskId, WorkTaskStatus newStatus, DateTimeOffset? completedAt = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.WorkTask>> GetTasksByAssigneeAsync(long employeeId, CancellationToken cancellationToken = default);
}

