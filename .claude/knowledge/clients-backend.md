# Clients Backend

## Overview
CRUD for Individual and Corporate clients. Each client is both an `ApplicationUser` (Identity) and a typed entity (`IndividualClient` or `CorporateClient`) linked via TPT inheritance. Creation is atomic: user + client record + welcome email with generated password all happen in one service call.

## Location
- `Controllers/ClientsController.cs` — REST endpoints, requires `Employee` or `Admin` role
- `Services/Clients/IClientService.cs` + `ClientService.cs` — GetAll, GetById, Deactivate (shared, type-agnostic)
- `Services/Clients/IndividualClients/IndividualClientService.cs` — Create/Update for individual clients
- `Services/Clients/CorporateClients/CorporateClientService.cs` — Create/Update for corporate clients
- `Repositories/Clients/IClientRepository.cs` + `ClientRepository.cs` — data access with EF Core
- `Mappers/Clients/ClientMapper.cs` — static mapper from entity → DTO
- `Services/Password/PasswordGenerator.cs` — generates 12-char random password for new clients
- `DTOs/Clients/IndividualClients/` — `CreateIndividualClientDto`, `UpdateIndividualClientDto`, `IndividualClientResponseDto`
- `DTOs/Clients/CorporateClients/` — `CreateCorporateClientDto`, `UpdateCorporateClientDto`, `CorporateClientResponseDto`
- `DTOs/Clients/ClientResponseDto.cs` — base response DTO

## How it works

### Client creation (Individual example)
1. `ClientsController.CreateIndividual` reads `createdByUserId` from the JWT (`ClaimTypes.NameIdentifier`)
2. `IndividualClientService.CreateAsync` checks uniqueness via `ExistsByEmailAsync` and `ExistsByEGNAsync` — throws `ConflictException` if duplicate
3. `PasswordGenerator.GeneratePassword()` creates a 12-char cryptographically random password
4. `UserManager.CreateAsync` creates the `ApplicationUser`; `AddToRoleAsync` assigns `Client` role
5. `IndividualClient` entity is created with `ClientId = user.Id` (PK = FK to AspNetUsers)
6. Repository saves; `EmailService.SendWelcomeEmailAsync` sends login credentials

### Client update (Individual example)
1. `GetByIdWithDetailsAsync` loads the `Client` base with its `User` navigation
2. TPT cast: `client is not IndividualClient ic` → throws `NotFoundException` if wrong type
3. Direct property assignment for non-identity fields (`ic.FirstName`, etc.)
4. `UserManager.SetEmailAsync` + `SetUserNameAsync` for email changes (never set directly — UserManager handles normalization)
5. Repository update + save

### TPT cast pattern in service layer
```csharp
var client = await _clientRepository.GetByIdWithDetailsAsync(id);

if (client is not IndividualClient ic)
    throw new NotFoundException("IndividualClient", id);
// now use ic
```
The repository returns a `Client` base. EF Core materializes the correct subtype. The service casts and throws if type doesn't match.

### ClientMapper
Static class with overloads — always use `ClientMapper.ToDto(client)` from controllers/services:
```csharp
// Specific overloads
ClientMapper.ToDto(IndividualClient) → IndividualClientResponseDto
ClientMapper.ToDto(CorporateClient)  → CorporateClientResponseDto

// Base overload — dispatches via switch expression
ClientMapper.ToDto(Client) → ClientResponseDto  // works on GetAll results
```

### PasswordGenerator
Generates a 12-char password guaranteed to contain: 1 uppercase, 1 lowercase, 1 digit, 1 special (`!@#$%`). Remaining 8 chars drawn from full charset. Positions shuffled with Fisher-Yates using `RandomNumberGenerator` (cryptographically secure).

## Key details

### RoleClaimType fix — MUST NOT be removed
`AddIdentity` overrides `RoleClaimType` at runtime, breaking `[Authorize(Roles)]`. Two settings are required in `Program.cs`:

```csharp
// In AddJwtBearer:
RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"

// After AddJwtBearer:
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    options.ClaimsIdentity.UserIdClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
});
```
Without both, `[Authorize(Roles = "Employee,Admin")]` always returns 403.

### Swashbuckle version
Downgraded from 10.2.0 to **6.9.0** (Microsoft.OpenApi 1.6.14). Version 10.x uses Microsoft.OpenApi 2.x which moved types out of `Microsoft.OpenApi.Models` and changed the Swagger security API. Stay on 6.9.0 until a stable 10.x pattern is established.

### UserManager for email updates
Never set `user.Email`, `user.UserName`, `user.NormalizedEmail`, `user.NormalizedUserName` directly. Always use:
```csharp
await _userManager.SetEmailAsync(user, newEmail);
await _userManager.SetUserNameAsync(user, newEmail);
```
UserManager handles normalization and triggers Identity validators.

### IRepository / IService base interfaces
Base interfaces live at:
- `Repositories/IRepository.cs` — `IRepository<T>` with `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `SaveChangesAsync`
- `Services/IService.cs` — `IService<TResponse>` with `GetAllClientsAsync`, `GetClientByIdAsync`, `DeactivateClientAsync`

Each feature interface extends the base: `IClientRepository : IRepository<Client>`

## Dependencies
- `Microsoft.AspNetCore.Identity` — UserManager for user creation and email updates
- `Swashbuckle.AspNetCore 6.9.0` — Swagger UI with Bearer auth definition

## How to extend

### Adding a new client type
1. Create entity inheriting from `Client` in `Entities/Clients/`
2. Add TPT config in `Data/Configurations/`
3. Add DTOs in `DTOs/Clients/[NewType]/`
4. Add `ToDto` overload in `ClientMapper` and add case to the base `ToDto` switch
5. Create `I[NewType]Service` + `[NewType]Service` following the existing pattern
6. Add endpoints in `ClientsController`
7. Register in `Config/ServiceExtensions.cs` and `Config/RepositoryExtensions.cs`
