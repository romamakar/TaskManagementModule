using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementModule.Data;
using TaskManagementModule.Data.Domain;
using TaskManagementModule.DTO;
using TaskManagementModule.Mapping;
using TaskManagementModule.Services;
using TaskManagementModule.Tests.Mocks;
using Xunit;

namespace TaskManagementModule.Tests.Services;

public sealed class TaskManagementServiceTests
{
    [Fact]
    public async Task CreateTaskAsync_CreatesTaskAndMapsRelatedEmployees()
    {
        using var testContext = TestContext.Create();
        var request = NewRequest(title: "  Prepare review  ");

        var result = await testContext.Service.CreateTaskAsync(request);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("Prepare review", result.Title);
        Assert.Equal(WorkTaskStatus.New, result.Status);
        Assert.Null(result.CompletedAt);
        Assert.Equal(1, result.CreatedByEmployeeId);
        Assert.Equal(2, result.AssignedToEmployeeId);
        Assert.Equal("Olena", result.CreatedByEmployee.FirstName);
        Assert.Equal("Andrii", result.AssignedToEmployee.FirstName);
        Assert.Equal(TimeSpan.Zero, result.PlannedStartAt.Offset);
        Assert.Equal(TimeSpan.Zero, result.DueAt.Offset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateTaskAsync_RejectsEmptyTitle(string title)
    {
        using var testContext = TestContext.Create();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            testContext.Service.CreateTaskAsync(NewRequest(title)));

        Assert.Equal(2, await testContext.DbContext.Tasks.CountAsync());
    }

    [Fact]
    public async Task CreateTaskAsync_RejectsDueDateBeforePlannedStart()
    {
        using var testContext = TestContext.Create();
        var request = NewRequest() with
        {
            PlannedStartAt = new DateTimeOffset(2026, 5, 12, 12, 0, 0, TimeSpan.Zero),
            DueAt = new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero)
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            testContext.Service.CreateTaskAsync(request));

        Assert.Equal(2, await testContext.DbContext.Tasks.CountAsync());
    }

    [Fact]
    public async Task CreateTaskAsync_RejectsAssignmentToCreator()
    {
        using var testContext = TestContext.Create();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            testContext.Service.CreateTaskAsync(NewRequest() with { AssignedToEmployeeId = 1 }));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(99)]
    public async Task CreateTaskAsync_RejectsInactiveOrMissingAssignee(long assigneeId)
    {
        using var testContext = TestContext.Create();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            testContext.Service.CreateTaskAsync(NewRequest() with { AssignedToEmployeeId = assigneeId }));
    }

    [Fact]
    public async Task ChangeStatusAsync_SetsCompletedAtOnlyForCompletedStatus()
    {
        using var testContext = TestContext.Create();
        var completionTime = new DateTimeOffset(2026, 5, 15, 10, 30, 0, TimeSpan.FromHours(3));

        var completed = await testContext.Service.ChangeStatusAsync(1, WorkTaskStatus.Completed, completionTime);
        var inProgress = await testContext.Service.ChangeStatusAsync(2, WorkTaskStatus.InProgress, completionTime);

        Assert.Equal(WorkTaskStatus.Completed, completed.Status);
        Assert.Equal(completionTime.ToUniversalTime(), completed.CompletedAt);
        Assert.Equal(WorkTaskStatus.InProgress, inProgress.Status);
        Assert.Null(inProgress.CompletedAt);
    }

    [Theory]
    [InlineData(WorkTaskStatus.Completed)]
    [InlineData(WorkTaskStatus.Cancelled)]
    public async Task ChangeStatusAsync_FinalStatusCannotTransitionToAnotherStatus(WorkTaskStatus finalStatus)
    {
        using var testContext = TestContext.Create();

        await testContext.Service.ChangeStatusAsync(1, finalStatus);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            testContext.Service.ChangeStatusAsync(1, WorkTaskStatus.InProgress));
    }

    [Fact]
    public async Task ChangeStatusAsync_ReapplyingCompletedPreservesOriginalCompletedAt()
    {
        using var testContext = TestContext.Create();
        var firstCompletionTime = new DateTimeOffset(2026, 5, 15, 10, 30, 0, TimeSpan.Zero);
        var secondCompletionTime = firstCompletionTime.AddDays(1);

        await testContext.Service.ChangeStatusAsync(1, WorkTaskStatus.Completed, firstCompletionTime);
        var result = await testContext.Service.ChangeStatusAsync(1, WorkTaskStatus.Completed, secondCompletionTime);

        Assert.Equal(firstCompletionTime, result.CompletedAt);
    }

    [Fact]
    public async Task ChangeStatusAsync_RejectsUnknownStatus()
    {
        using var testContext = TestContext.Create();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            testContext.Service.ChangeStatusAsync(1, (WorkTaskStatus)99));
    }

    [Fact]
    public async Task ChangeStatusAsync_ThrowsWhenTaskDoesNotExist()
    {
        using var testContext = TestContext.Create();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            testContext.Service.ChangeStatusAsync(99, WorkTaskStatus.InProgress));
    }

    [Fact]
    public async Task GetTasksByAssigneeAsync_ReturnsMappedTasksOrderedByDueDate()
    {
        using var testContext = TestContext.Create();
        testContext.DbContext.Tasks.AddRange(
            NewDomainTask("Later task", new DateTimeOffset(2026, 1, 5, 12, 0, 0, TimeSpan.Zero)),
            NewDomainTask("Sooner task", new DateTimeOffset(2026, 1, 3, 12, 0, 0, TimeSpan.Zero)));
        await testContext.DbContext.SaveChangesAsync();

        var tasks = (await testContext.Service.GetTasksByAssigneeAsync(2)).ToList();

        Assert.Equal(3, tasks.Count);
        Assert.Equal("Sooner task", tasks[0].Title);
        Assert.Equal("Later task", tasks[1].Title);
        Assert.Equal("Prepare monthly report", tasks[2].Title);
        Assert.All(tasks, task => Assert.Equal("Andrii", task.AssignedToEmployee.FirstName));
    }

    private static CreateTaskRequest NewRequest(string title = "Test task") => new(
        Title: title,
        Description: "Description",
        CreatedByEmployeeId: 1,
        AssignedToEmployeeId: 2,
        PlannedStartAt: new DateTimeOffset(2026, 5, 10, 9, 0, 0, TimeSpan.FromHours(3)),
        DueAt: new DateTimeOffset(2026, 5, 15, 17, 0, 0, TimeSpan.FromHours(3)));

    private static WorkTask NewDomainTask(string title, DateTimeOffset dueAt) => new()
    {
        Title = title,
        CreatedByEmployeeId = 1,
        AssignedToEmployeeId = 2,
        PlannedStartAt = dueAt.AddDays(-1),
        DueAt = dueAt,
        Status = WorkTaskStatus.New,
        CreatedAt = DateTimeOffset.UtcNow
    };

   
}
