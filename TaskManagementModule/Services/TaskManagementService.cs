using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagementModule.Data;
using TaskManagementModule.Data.Domain;
using TaskManagementModule.DTO;

namespace TaskManagementModule.Services;

public sealed class TaskManagementService(TaskManagementDbContext dbContext, IMapper mapper) : ITaskManagementService
{
    public async Task<Models.WorkTask> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Title);

        if (request.DueAt < request.PlannedStartAt)
        {
            throw new ArgumentException("Due date cannot be earlier than the planned start date.", nameof(request));
        }

        if (request.CreatedByEmployeeId == request.AssignedToEmployeeId)
        {
            throw new InvalidOperationException("A task cannot be assigned to its creator.");
        }

        var assigneeIsActive = await dbContext.Employees.AnyAsync(employee => employee.Id == request.AssignedToEmployeeId && employee.IsActive, cancellationToken);
        if (!assigneeIsActive)
        {
            throw new InvalidOperationException("A task cannot be assigned to an inactive or missing employee.");
        }

        var task = new WorkTask
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            CreatedByEmployeeId = request.CreatedByEmployeeId,
            AssignedToEmployeeId = request.AssignedToEmployeeId,
            PlannedStartAt = request.PlannedStartAt.ToUniversalTime(),
            DueAt = request.DueAt.ToUniversalTime(),
            Status = WorkTaskStatus.New,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        var createdTask = await dbContext.Tasks.AsNoTracking()
            .Include(item => item.CreatedByEmployee)
            .Include(item => item.AssignedToEmployee)
            .SingleAsync(item => item.Id == task.Id, cancellationToken);

        return mapper.Map<Models.WorkTask>(createdTask);
    }

    public async Task<Models.WorkTask> ChangeStatusAsync(long taskId, WorkTaskStatus newStatus, DateTimeOffset? completedAt = null, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(newStatus))
        {
            throw new ArgumentOutOfRangeException(nameof(newStatus));
        }

        var task = await dbContext.Tasks
            .Include(item => item.CreatedByEmployee)
            .Include(item => item.AssignedToEmployee)
            .SingleOrDefaultAsync(item => item.Id == taskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {taskId} was not found.");

        if (task.Status == newStatus)
        {
            return mapper.Map<Models.WorkTask>(task);
        }

        if ((task.Status is WorkTaskStatus.Completed or WorkTaskStatus.Cancelled) && task.Status != newStatus)
        {
            throw new InvalidOperationException("Completed and cancelled tasks cannot transition to another status.");
        }

        task.Status = newStatus;
        task.CompletedAt = newStatus == WorkTaskStatus.Completed ? (completedAt ?? DateTimeOffset.UtcNow).ToUniversalTime() : null;

        await dbContext.SaveChangesAsync(cancellationToken);
        return mapper.Map<Models.WorkTask>(task);
    }

    public async Task<IEnumerable<Models.WorkTask>> GetTasksByAssigneeAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        var tasks = await dbContext.Tasks.Include(x => x.AssignedToEmployee).Include(x => x.CreatedByEmployee).AsNoTracking()
            .Where(task => task.AssignedToEmployeeId == employeeId).OrderBy(task => task.DueAt).ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<Models.WorkTask>>(tasks);
    }
}
