namespace TaskManagementModule.Data.Domain;

public sealed class Employee : Entity<long>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<WorkTask> CreatedTasks { get; set; } = new List<WorkTask>();
    public ICollection<WorkTask> AssignedTasks { get; set; } = new List<WorkTask>();
}
