# Task Management Module

Модуль внутрішньої CRM для роботи із задачами співробітників. Це бібліотека backend/data layer на .NET 10, EF Core та PostgreSQL; HTTP API і UI до складу не входять.

## Можливості

- Моделі співробітників і задач, зв’язки автора та виконавця.
- EF Core Fluent API-конфігурація, індекси, обмеження та початкова міграція `InitialCreate`.
- Seed-дані: 3 співробітники та 2 задачі.
- Сервісні операції: створення задачі, зміна статусу та список задач за виконавцем.
- AutoMapper для перетворення доменних сутностей у `Models.WorkTask` та `Models.Employee`.

## Встановлення та реєстрація

Додайте посилання на `TaskManagementModule` у host-проєкт. У його `Program.cs` передайте connection string до методу розширення:

```csharp
using TaskManagementModule.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TaskManagement")
	?? throw new InvalidOperationException("Connection string 'TaskManagement' was not configured.");

builder.Services.AddTaskManagementServices(connectionString);
```

Приклад конфігурації `appsettings.json` для локальної розробки:

```json
{
  "ConnectionStrings": {
	"TaskManagement": "Host=localhost;Port=5432;Database=task_management;Username=postgres;Password=your-local-password"
  }
}
```


`AddTaskManagementServices` реєструє `TaskManagementDbContext` із PostgreSQL-провайдером, профіль AutoMapper і `ITaskManagementService`. Сервіс можна отримувати через DI у вашому host-коді:

```csharp
using TaskManagementModule.Services;
using TaskManagementModule.DTO;
using TaskManagementModule.Models;

public sealed class TaskWorkflow(ITaskManagementService taskService)
{
	public Task<WorkTask> CreateAsync(CancellationToken cancellationToken) =>
		taskService.CreateTaskAsync(
			new CreateTaskRequest(
				Title: "Prepare monthly report",
				Description: "Compile department metrics.",
				CreatedByEmployeeId: 1,
				AssignedToEmployeeId: 2,
				PlannedStartAt: DateTimeOffset.UtcNow,
				DueAt: DateTimeOffset.UtcNow.AddDays(7)),
			cancellationToken);
}
```

Імпортуйте `TaskManagementModule.DTO` для `CreateTaskRequest` та `TaskManagementModule.Data.Domain` для `WorkTaskStatus`, якщо вони потрібні у host-коді.

## Міграції та база даних

Міграції зберігаються у `TaskManagementModule/Data/Migrations`. У репозиторії наразі є початкова міграція `InitialCreate`. Для роботи EF CLI встановіть `dotnet-ef` тієї ж основної версії, що й EF Core (у проєкті використовується EF Core 10), і додайте `Microsoft.EntityFrameworkCore.Design` у startup-проєкт, якщо EF CLI цього вимагає.

У цьому рішенні окремого startup-проєкту немає. Створіть або використайте host-проєкт, у якому в `Program.cs` викликається `AddTaskManagementServices(connectionString)`. З кореня solution створіть нову міграцію так:

```powershell
dotnet ef migrations add AddTaskField `
  --project .\TaskManagementModule\TaskManagementModule.csproj `
  --startup-project .\YourHost\YourHost.csproj `
  --context TaskManagementDbContext `
  --output-dir Data\Migrations
```

Застосуйте її до бази даних:

```powershell
dotnet ef database update `
  --project .\TaskManagementModule\TaskManagementModule.csproj `
  --startup-project .\YourHost\YourHost.csproj `
  --context TaskManagementDbContext
```

Замініть `YourHost` на шлях до вашого startup-проєкту. EF CLI використовує його DI-конфігурацію для створення `TaskManagementDbContext`; окремий `TaskManagementDbContextFactory` у модулі не використовується.

## Операції сервісу

`ITaskManagementService` доступний через DI і має такі методи:

- `CreateTaskAsync(CreateTaskRequest)` — створює задачу зі статусом `New` і повертає `Models.WorkTask` із даними автора та виконавця.
- `ChangeStatusAsync(taskId, newStatus, completedAt?)` — змінює статус і повертає `Models.WorkTask`. Якщо `newStatus` — `Completed`, `CompletedAt` встановлюється з переданого значення або поточного UTC часу. Для інших статусів `CompletedAt` очищується.
- `GetTasksByAssigneeAsync(employeeId)` — повертає список `Models.WorkTask` виконавця, відсортований за дедлайном; у результаті доступні моделі автора та виконавця.

Приклад зміни статусу:

```csharp
using TaskManagementModule.Data.Domain;

var completed = await taskService.ChangeStatusAsync(
	taskId,
	WorkTaskStatus.Completed,
	cancellationToken: cancellationToken);
```

## Бізнес-правила

- `DueAt` не може бути раніше `PlannedStartAt`.
- Призначити задачу неактивному співробітнику не можна.
- Автор і виконавець задачі мають бути різними співробітниками.
- `CompletedAt` заповнюється лише для статусу `Completed`.
- `Completed` і `Cancelled` — фінальні статуси; змінити їх на інший статус не можна.
- Часові значення зберігаються в UTC.
