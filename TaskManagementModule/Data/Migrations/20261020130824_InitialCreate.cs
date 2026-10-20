using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManagementModule.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedByEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    AssignedToEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    PlannedStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.Id);
                    table.CheckConstraint("ck_tasks_completed_at_matches_status", "(\"Status\" = 2 AND \"CompletedAt\" IS NOT NULL) OR (\"Status\" <> 2 AND \"CompletedAt\" IS NULL)");
                    table.CheckConstraint("ck_tasks_creator_differs_from_assignee", "\"CreatedByEmployeeId\" <> \"AssignedToEmployeeId\"");
                    table.CheckConstraint("ck_tasks_due_at_after_planned_start_at", "\"DueAt\" >= \"PlannedStartAt\"");
                    table.ForeignKey(
                        name: "FK_tasks_employees_AssignedToEmployeeId",
                        column: x => x.AssignedToEmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tasks_employees_CreatedByEmployeeId",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "employees",
                columns: new[] { "Id", "Email", "FirstName", "IsActive", "LastName" },
                values: new object[,]
                {
                    { 1L, "olena.koval@example.com", "Olena", true, "Koval" },
                    { 2L, "andrii.shevchenko@example.com", "Andrii", true, "Shevchenko" },
                    { 3L, "iryna.melnyk@example.com", "Iryna", false, "Melnyk" }
                });

            migrationBuilder.InsertData(
                table: "tasks",
                columns: new[] { "Id", "AssignedToEmployeeId", "CompletedAt", "CreatedAt", "CreatedByEmployeeId", "Description", "DueAt", "PlannedStartAt", "Status", "Title" },
                values: new object[,]
                {
                    { 1L, 2L, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1L, "Compile the department metrics.", new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0, "Prepare monthly report" },
                    { 2L, 1L, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2L, "Check the latest onboarding process.", new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, "Review onboarding checklist" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_employees_Email",
                table: "employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_IsActive",
                table: "employees",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_AssignedToEmployeeId_Status",
                table: "tasks",
                columns: new[] { "AssignedToEmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tasks_CreatedByEmployeeId",
                table: "tasks",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_DueAt",
                table: "tasks",
                column: "DueAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "employees");
        }
    }
}
