# Employees Backend

## Overview
Admin-only CRUD-lite for Employee accounts: create, list, get by id, activate, deactivate. Employee is **not** a separate entity — it is an `ApplicationUser` with role `"Employee"` only, the same pattern Clients used before `Clients` got its own TPT table. No new entity, no migration.

## Location
- `Controllers/EmployeesController.cs` — `POST /api/employees`, `GET /api/employees`, `GET /api/employees/{id}`, `PATCH /api/employees/{id}/deactivate`, `PATCH /api/employees/{id}/activate`; class-level `[Authorize(Roles = "Admin")]`
- `Services/Employees/IEmployeeService.cs` + `EmployeeService.cs` — business logic
- `Repositories/Employees/IEmployeeRepository.cs` + `EmployeeRepository.cs` — data access against `ApplicationDbContext` + `UserManager<ApplicationUser>`
- `Mappers/Employees/EmployeeMapper.cs` — static `ToDto(ApplicationUser)`
- `DTOs/Employees/CreateEmployeeDto.cs`, `UpdateEmployeeDto.cs`, `EmployeeResponseDto.cs`

## How it works

### Create (`CreateEmployeeAsync`)
1. `IEmployeeRepository.ExistsByEmailAsync(dto.Email)` → throws `ConflictException` if duplicate
2. `IPasswordGenerator.GeneratePassword()` — same generator used by Clients, no duplicate password logic
3. New `ApplicationUser` built from `dto.FirstName/LastName/Email`; `UserManager.CreateAsync(user, password)` → throws `ValidationException` (joined Identity errors) if creation fails
4. `UserManager.AddToRoleAsync(user, "Employee")` — assigns the role; this is the only thing that makes the user "an Employee" (no FK, no extra row)
5. `IEmailService.SendWelcomeEmailAsync(dto.Email, dto.FirstName, password)` — same welcome email used by Clients
6. Returns `EmployeeMapper.ToDto(user)`

### Get All / Get By Id
- `GetAllEmployeesAsync` → `IEmployeeRepository.GetAllAsync()`, which calls `UserManager.GetUsersInRoleAsync("Employee")` — **not** a manual `AspNetUserRoles` join. `UserManager` already knows how to resolve role membership; reusing it avoids hand-rolling the join.
- `GetEmployeeByIdAsync(id)` → private helper `GetEmployeeUserAsync(id)` (see below)

### Activate / Deactivate
Both go through the same private helper, then flip `IsActive` and persist via `UserManager.UpdateAsync` (not the repository's generic `UpdateAsync` — `UserManager` is the correct Identity-aware path for mutating an `ApplicationUser`):
```csharp
public async Task DeactivateEmployeeAsync(Guid id, Guid requestingAdminId)
{
    var user = await GetEmployeeUserAsync(id);
    user.IsActive = false;
    await _userManager.UpdateAsync(user);
}
```
`ActivateEmployeeAsync` is identical but sets `IsActive = true`.

### Shared role-guard helper
```csharp
private async Task<ApplicationUser> GetEmployeeUserAsync(Guid id)
{
    var user = await _employeeRepository.GetByIdAsync(id);
    if (user == null || !await _userManager.IsInRoleAsync(user, "Employee"))
        throw new NotFoundException("Employee", id);

    return user;
}
```
`IEmployeeRepository.GetByIdAsync(id)` is a **plain** `ApplicationUser` lookup (`_context.Users.FindAsync(id)`) with no role filter — the role check happens here, in the service, via `UserManager.IsInRoleAsync`. This is deliberate: it means an Admin or Client id passed to `GET /api/employees/{id}` (or activate/deactivate) returns 404, not the wrong user's data — you cannot activate/deactivate an Admin or Client through this endpoint.

## Key details
- **No ownership check anywhere in `EmployeeService`** — unlike Employee→Client (`Client.CreatedByUserId` filtering), any Admin can view/manage any employee. There is no employee-managing-employee concept, so `requestingAdminId` is accepted by `DeactivateEmployeeAsync`/`ActivateEmployeeAsync` for future logging but is not currently used for any access check.
- **Role membership IS the entity** — there is no `Employees` table. `GetAllAsync` filters via `UserManager.GetUsersInRoleAsync`, not a `Set<T>()` query, because there is no `Employee`-typed `DbSet`.
- **`IActivityLogService` calls deliberately omitted** — `feature/activity-log` (teammate's parallel branch) has not been merged into `develop`, so `IActivityLogService` does not exist in this codebase yet. `CreateEmployeeAsync`, `DeactivateEmployeeAsync`, and `ActivateEmployeeAsync` do **not** call it. This is the one place this feature diverges from the `CONVENTIONS.md` example (`_activityLogService.LogAsync(...)` after every operation) — intentional, not an oversight. Wire it in once that branch merges; see `DECISIONS.md` 2026-06-20 entry.
- `IEmployeeService` does **not** extend `IService<T>`/`IGenericService<T>` — matches the actual `IClientService`/`IIndividualClientService` pattern in this codebase (plain feature-specific interface), not the generic-base example shown in `CONVENTIONS.md`.
- `NotFoundException("Employee", id)` is thrown for three distinct real conditions (user doesn't exist; user exists but is an Admin; user exists but is a Client) — all collapse to the same 404, which is correct: the caller has no business knowing which of the three happened.

## Code snippets

### Create
```csharp
public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, Guid createdByUserId)
{
    if (await _employeeRepository.ExistsByEmailAsync(dto.Email))
        throw new ConflictException("An employee with this email already exists.");

    var password = _passwordGenerator.GeneratePassword();

    var user = new ApplicationUser
    {
        Email = dto.Email,
        UserName = dto.Email,
        FirstName = dto.FirstName,
        LastName = dto.LastName
    };

    var result = await _userManager.CreateAsync(user, password);
    if (!result.Succeeded)
        throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));

    await _userManager.AddToRoleAsync(user, "Employee");

    await _emailService.SendWelcomeEmailAsync(dto.Email, dto.FirstName, password);

    return EmployeeMapper.ToDto(user);
}
```

### Activate / Deactivate
```csharp
public async Task DeactivateEmployeeAsync(Guid id, Guid requestingAdminId)
{
    var user = await GetEmployeeUserAsync(id);
    user.IsActive = false;
    await _userManager.UpdateAsync(user);
}

public async Task ActivateEmployeeAsync(Guid id, Guid requestingAdminId)
{
    var user = await GetEmployeeUserAsync(id);
    user.IsActive = true;
    await _userManager.UpdateAsync(user);
}
```

## Dependencies
- None beyond the existing ASP.NET Identity / `UserManager<ApplicationUser>` stack already wired for Auth and Clients.

## How to extend

### Add `UpdateEmployeeAsync` (profile edit)
1. Add `Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto dto)` to `IEmployeeService`
2. Reuse `GetEmployeeUserAsync(id)` to load + role-guard
3. Set `user.FirstName`/`user.LastName` directly; use `_userManager.SetEmailAsync(user, dto.Email)` + `SetUserNameAsync` for email changes — **never** set `user.Email`/`user.UserName` directly (same rule as `IndividualClientService`, see `.claude/knowledge/clients/backend.md`)
4. `await _userManager.UpdateAsync(user)`
5. Add `[HttpPut("{id:guid}")]` to `EmployeesController`

### Wire in ActivityLog once `feature/activity-log` merges
Add `IActivityLogService _activityLogService` to the constructor and call `LogAsync(requestingAdminId/createdByUserId, "CreateEmployee"/"DeactivateEmployee"/"ActivateEmployee", "Employee", user.Id, details)` at the end of each of the three methods — `requestingAdminId`/`createdByUserId` parameters already exist on the signatures for this purpose.
