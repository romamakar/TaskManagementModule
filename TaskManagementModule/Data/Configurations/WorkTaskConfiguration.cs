using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagementModule.Data.Domain;

namespace TaskManagementModule.Data.Configurations;

public sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    private static readonly DateTimeOffset SeedTime = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("tasks", table =>
        {
            table.HasCheckConstraint("ck_tasks_due_at_after_planned_start_at", "\"DueAt\" >= \"PlannedStartAt\"");
            table.HasCheckConstraint(
                "ck_tasks_completed_at_matches_status",
                "(\"Status\" = 2 AND \"CompletedAt\" IS NOT NULL) OR (\"Status\" <> 2 AND \"CompletedAt\" IS NULL)");
            table.HasCheckConstraint(
                "ck_tasks_creator_differs_from_assignee",
                "\"CreatedByEmployeeId\" <> \"AssignedToEmployeeId\"");
        });

        builder.HasKey(task => task.Id);
        builder.Property(task => task.Title)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(task => task.Description)
            .HasMaxLength(4000);
        builder.Property(task => task.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(task => task.CreatedAt)
            .IsRequired();

        builder.HasOne(task => task.CreatedByEmployee)
            .WithMany(employee => employee.CreatedTasks)
            .HasForeignKey(task => task.CreatedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(task => task.AssignedToEmployee)
            .WithMany(employee => employee.AssignedTasks)
            .HasForeignKey(task => task.AssignedToEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(task => new { task.AssignedToEmployeeId, task.Status });
        builder.HasIndex(task => task.DueAt);

        builder.HasData(
            new WorkTask
            {
                Id = 1L,
                Title = "Prepare monthly report",
                Description = "Compile the department metrics.",
                CreatedByEmployeeId = 1L,
                AssignedToEmployeeId = 2L,
                PlannedStartAt = SeedTime,
                DueAt = SeedTime.AddDays(7),
                Status = WorkTaskStatus.New,
                CreatedAt = SeedTime
            },
            new WorkTask
            {
                Id = 2L,
                Title = "Review onboarding checklist",
                Description = "Check the latest onboarding process.",
                CreatedByEmployeeId = 2L,
                AssignedToEmployeeId = 1L,
                PlannedStartAt = SeedTime.AddDays(1),
                DueAt = SeedTime.AddDays(4),
                Status = WorkTaskStatus.InProgress,
                CreatedAt = SeedTime
            });
    }
}
