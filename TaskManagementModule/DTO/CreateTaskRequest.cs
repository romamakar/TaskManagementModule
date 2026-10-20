namespace TaskManagementModule.DTO;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    long CreatedByEmployeeId,
    long AssignedToEmployeeId,
    DateTimeOffset PlannedStartAt,
    DateTimeOffset DueAt);
