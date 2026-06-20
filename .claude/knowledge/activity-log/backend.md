# Activity Log Backend

## Overview
Audit log of Employee/Admin operations. The `ActivityLog` entity/table already existed from the initial migration but was unused until this feature — it wires up the repository, service, controller, and the actual `LogAsync` call sites across existing services. Logging failures must never break the operation being logged.

## Location
- `Entities/ActivityLog.cs` — `UserId, Action, EntityType, EntityId, Timestamp, Details` (pre-existing, unchanged)
- `Repositories/ActivityLogs/IActivityLogRepository.cs` + `ActivityLogRepository.cs` — extends `IRepository<ActivityLog>`, no extra methods
- `Services/ActivityLogs/IActivityLogService.cs` + `ActivityLogService.cs` — `LogAsync`, `GetAllLogsAsync`
- `Controllers/ActivityLogsController.cs` — `GET /api/activity-logs`, Admin only
- `DTOs/ActivityLogs/ActivityLogResponseDto.cs`
- `Mappers/ActivityLogs/ActivityLogMapper.cs`
- `Data/Seeders/DataSeeder.cs` — dummy seed rows for manual filter testing

## How it works

### LogAsync — swallow-exception design
```csharp
public async Task LogAsync(Guid userId, string action, string entityType, Guid entityId, string? details = null)
{
    try
    {
        var log = new ActivityLog { UserId = userId, Action = action, EntityType = entityType, EntityId = entityId, Details = details };
        await _activityLogRepository.AddAsync(log);
        await _activityLogRepository.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to write activity log for action {Action} on {EntityType} {EntityId}", action, entityType, entityId);
    }
}
```
`LogAsync` **never throws**. Any failure (DB unavailable, FK violation, whatever) is caught, logged via the injected `ILogger<ActivityLogService>`, and swallowed. Callers never wrap `LogAsync` in try/catch — it's safe to `await` directly as the last statement before `return` in a business method.

### GetAllLogsAsync
```csharp
public async Task<IEnumerable<ActivityLogResponseDto>> GetAllLogsAsync()
{
    var logs = await _activityLogRepository.GetAllAsync();
    return logs.Select(ActivityLogMapper.ToDto);
}
```
`ActivityLogRepository.GetAllAsync()` eager-loads `.Include(al => al.User)` and orders `.OrderByDescending(al => al.Timestamp)` — newest first, and `User` is always populated so the mapper can build `UserName` without a second query.

### ActivityLogMapper
```csharp
public static ActivityLogResponseDto ToDto(ActivityLog log) => new()
{
    Id = log.Id,
    UserId = log.UserId,
    UserName = $"{log.User.FirstName} {log.User.LastName}",
    Action = log.Action,
    EntityType = log.EntityType,
    EntityId = log.EntityId,
    Timestamp = log.Timestamp,
    Details = log.Details
};
```

### Call sites — which services log, and which don't yet
`LogAsync` is wired into, with the call placed right before the final `return` (i.e. after the entity is already saved — logging confirmed success, not an attempt):

| Service | Methods | Action string |
|---|---|---|
| `EmployeeService` | `CreateEmployeeAsync`, `DeactivateEmployeeAsync`, `ActivateEmployeeAsync` | `CreateEmployee`, `DeactivateEmployee`, `ActivateEmployee` |
| `IndividualClientService` | `CreateAsync`, `UpdateAsync` | `CreateClient`, `UpdateClient` |
| `CorporateClientService` | `CreateAsync`, `UpdateAsync` | `CreateClient`, `UpdateClient` |
| `BankAccountService` | `OpenAccountAsync`, `CloseAccountAsync`, `DepositAsync`, `WithdrawAsync` | `OpenAccount`, `CloseAccount`, `Deposit`, `Withdraw` |
| `CreditService` | `GrantConsumerCreditAsync`, `GrantMortgageCreditAsync`, `UpdateConsumerCreditAsync`, `UpdateMortgageCreditAsync` | `GrantConsumerCredit`, `GrantMortgageCredit`, `UpdateConsumerCredit`, `UpdateMortgageCredit` |

**Not wired — deferred:**
- `ClientService.DeactivateAsync`/`ActivateAsync`
- `BankAccountService.DeleteAccountAsync`
- `CreditServiceService` (Create/Update/Delete)

All three are excluded for the same reason: none of these methods currently accept a `requestingUserId`/`createdByUserId` (or any user-identifying) parameter in their signature. Adding logging there requires a signature change first (and updating every controller/caller) — out of scope for this feature, deferred as a follow-up.

### Dummy seed data (DataSeeder)
Idempotency check before inserting:
```csharp
var dummySeeded = await context.ActivityLogs
    .AnyAsync(al => al.Details != null && al.Details.StartsWith("[DUMMY]"));

if (!dummySeeded)
{
    // ... build and insert 12 rows
    await context.ActivityLogs.AddRangeAsync(dummyLogs);
    await context.SaveChangesAsync();
}
```
Every `Details` value is prefixed `"[DUMMY] "` (e.g. `"[DUMMY] Deposited 500.00 BGN"`). `EntityId` is `Guid.NewGuid()` — it does **not** need to point to a real row; the activity log is an audit trail, not an FK to the target entity. Rows are spread across `Timestamp` (today, hours ago, days ago, weeks ago) using the already-seeded `admin`, `admin2`, `employee1`, `employee2` users so the User/Action/date-range filters have real data to exercise.

## Key details
- `ActivityLogRepository`/`IActivityLogRepository` extend the plain `IRepository<ActivityLog>` with **no extra methods** — `GetAllAsync` is overridden purely for the `.Include` + ordering, everything else is the standard implementation.
- `EntityId` on `ActivityLog` is just a `Guid` column, not a real FK to `Clients`/`BankAccounts`/`Credits`/etc. — never assume it resolves to a row in another table.
- Constructor injection pattern for every service that now logs: `IActivityLogService activityLogService` added as the last constructor parameter, alongside whatever was already injected (e.g. `BankAccountService(IBankAccountRepository, IClientRepository, IActivityLogService)`).
- `ActivityLogsController` follows the exact thin-controller shape of `EmployeesController`/`ClientsController` — `[Authorize(Roles = "Admin")]` at class level, single `GET` action, no business logic.

## Dependencies
- `Microsoft.Extensions.Logging.ILogger<T>` — already part of the ASP.NET Core host, no new package.

## How to extend

### Add logging to a new service method
1. Inject `IActivityLogService` into the service's constructor if not already present
2. Place `await _activityLogService.LogAsync(userId, "ActionName", "EntityType", entityId, $"Human-readable details");` immediately before the method's final `return`, after all persistence (`SaveChangesAsync`) has happened
3. No try/catch needed at the call site — `LogAsync` never throws

### Retrofit one of the deferred methods (ClientService activate/deactivate, BankAccountService.DeleteAccountAsync, CreditServiceService)
1. Add a `Guid requestingUserId` (or `createdByUserId`) parameter to the method's signature
2. Update every controller action and any other caller to pass the user id (extracted via `Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`, same as everywhere else in the project)
3. Update any existing unit/integration tests that call the method with the old signature
4. Add the `LogAsync` call following the same placement pattern as the wired methods above
