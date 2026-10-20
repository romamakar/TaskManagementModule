namespace TaskManagementModule.Data.Domain;

public sealed class WorkTask : Entity<long>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long CreatedByEmployeeId { get; set; }
    public long AssignedToEmployeeId { get; set; }
    public DateTimeOffset PlannedStartAt { get; set; }
    public DateTimeOffset DueAt { get; set; }
    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.New;
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Employee CreatedByEmployee { get; set; } = null!;
    public Employee AssignedToEmployee { get; set; } = null!;
}
