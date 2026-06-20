# Settings Backend

## Overview
Self-service change password (all roles) + profile name update (Admin/Employee only). No repository — operates directly via `UserManager<ApplicationUser>`.

## Location
- `DTOs/Settings/ChangePasswordDto.cs`, `UpdateProfileDto.cs`, `ProfileResponseDto.cs`
- `Services/Settings/ISettingsService.cs` + `SettingsService.cs`
- `Controllers/SettingsController.cs`

## How it works

### GetProfileAsync / UpdateProfileAsync
1. `GetUserAsync(userId)` — `UserManager.FindByIdAsync(userId.ToString())`, throws `NotFoundException("User", userId)` if null
2. `UpdateProfileAsync` assigns `FirstName`/`LastName` directly on the `ApplicationUser`, then `UserManager.UpdateAsync(user)` persists
3. Both return `ToDtoAsync(user)` — loads roles via `UserManager.GetRolesAsync(user)` and builds `ProfileResponseDto` inline (no mapper)

### ChangePasswordAsync
`UserManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword)` validates the current password internally (no manual check needed) and enforces Identity's configured password rules on the new one. On failure, joined `IdentityResult.Errors` are thrown as a `ValidationException`.

### Controller role split
Class-level `[Authorize]` (any authenticated user), with endpoint-level overrides:
- `GET /api/settings/profile`, `PUT /api/settings/profile` → `[Authorize(Roles = "Admin,Employee")]`
- `PATCH /api/settings/password` → no extra attribute, inherits class-level `[Authorize]` only — every role can change their own password

## Key details
- **MUST NOT extend profile-editing to Client role without also addressing the TPT name duplication.** `IndividualClient`/`CorporateClient` store their own `FirstName`/`LastName`, separate from `ApplicationUser.FirstName`/`LastName`. `ClientMapper`/`ClientsListPage`/`ClientDetailPage` read the Client-specific TPT copy, never `ApplicationUser`. A Client self-editing their name via Settings would update `ApplicationUser` only, silently desyncing from what Employees/Admins see in the Clients list. This is why Client only gets password change here — ties back to `PROJECT.md`'s "Client: Cannot modify any data" principle.
- ActivityLog wired in for both `UpdateProfile` and `ChangePassword` — cheap to add since the actor's own `userId` is already on hand (taken from the JWT in the controller, no extra lookup).
- No separate mapper class — `ProfileResponseDto` (5 fields) is built inline in `ToDtoAsync`, unlike `ClientMapper`/`EmployeeMapper`/`ActivityLogMapper`.
- No email-change support — `UpdateProfileDto` has no `Email` field; changing login email would need a re-verification flow that doesn't exist yet.

## Code snippets

### ChangePasswordAsync
```csharp
public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
{
    var user = await GetUserAsync(userId);

    var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
    if (!result.Succeeded)
        throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));

    await _activityLogService.LogAsync(userId, "ChangePassword", "User", userId, $"Changed password for {user.Email}");
}
```

### Controller — role-attribute split
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    [HttpGet("profile")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GetProfile() { ... }

    [HttpPut("profile")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto) { ... }

    [HttpPatch("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto) { ... }
}
```

## Dependencies
None beyond the existing `UserManager<ApplicationUser>` stack.

## How to extend

### If Client-side profile editing is ever needed
Either:
1. **(Recommended — minimal blast radius)** Inside `UpdateProfileAsync`, check if the user has the `Client` role and, if so, also update the matching `IndividualClient`/`CorporateClient` row's name fields (load via `IClientRepository`, set `FirstName`/`LastName`, save) — keeps both copies in sync without touching any read path.
2. Make `ClientsListPage`/`ClientDetailPage`/`ClientMapper` read the name from `ApplicationUser` instead of the TPT-specific field — larger change, touches the Clients feature's established read path.

### Add email-change support
Would need a verification flow (generate token, send confirmation email, confirm via a new endpoint) before allowing `UserManager.SetEmailAsync`/`SetUserNameAsync` to take effect — do not add a bare `Email` field to `UpdateProfileDto` without it.
