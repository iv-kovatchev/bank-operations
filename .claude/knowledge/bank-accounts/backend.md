# Bank Accounts Backend

## Overview
CRUD + financial transactions for bank accounts. Each account belongs to a `Client` (1:N), has a soft-delete flag, and an `AccountStatus` (`Active`/`Closed`). Employees can only operate on accounts of clients they created; Admins bypass all ownership checks.

## Location
- `Controllers/BankAccountsController.cs` — REST endpoints (open/list/close/delete/deposit/withdraw)
- `Services/BankAccounts/IBankAccountService.cs` + `BankAccountService.cs` — business logic
- `Repositories/BankAccounts/IBankAccountRepository.cs` + `BankAccountRepository.cs` — data access
- `Mappers/BankAccounts/BankAccountMapper.cs` — static entity → DTO mapper
- `Entities/BankAccount.cs` — entity (`IBAN`, `Balance`, `Status`, `ClientId`, `IsDeleted`, `CreatedByUserId`)
- `Enums/AccountStatus.cs` — `Active` | `Closed`
- `DTOs/BankAccounts/CreateBankAccountDto.cs` — `IBAN` (`[Required]`), `InitialBalance` (`[Range(0, max)]`)
- `DTOs/BankAccounts/TransactionDto.cs` — `Amount` (`[Range(0.01, double.MaxValue)]`)
- `DTOs/BankAccounts/BankAccountResponseDto.cs` — `Id, IBAN, Balance, Status (string), ClientId, CreatedAt, CreatedByUserId`

## How it works

### Routes
Two route bases on the same controller via `[Route("api/clients/{clientId}/accounts")]` + absolute overrides (`~/api/...`):
- `GET /api/clients/{clientId}/accounts` — list (client-scoped)
- `POST /api/clients/{clientId}/accounts` — open (client-scoped, Employee/Admin)
- `PATCH /api/accounts/{id}/close` — Employee/Admin
- `DELETE /api/accounts/{id}` — Admin only (soft delete)
- `PATCH /api/accounts/{id}/deposit` — Employee/Admin
- `PATCH /api/accounts/{id}/withdraw` — Employee/Admin

### Open account (`OpenAccountAsync`)
1. `_clientRepository.GetByIdWithDetailsAsync(clientId)` — eager-loads `client.User` (needed for `IsActive`)
2. Throws `ValidationException` if `!client.User.IsActive` ("Cannot open an account for an inactive client.")
3. Throws `ConflictException` if `ExistsByIbanAsync(dto.IBAN)` is true
4. Creates `BankAccount` with `Status = AccountStatus.Active`, saves, returns `BankAccountMapper.ToDto(account)`

### Ownership check pattern (Close/Deposit/Withdraw)
All three take `(id, requestingUserId, isAdmin)`:
```csharp
var account = await _bankAccountRepository.GetByIdWithClientAsync(id)
    ?? throw new NotFoundException("BankAccount", id);

if (!isAdmin && account.Client.CreatedByUserId != requestingUserId)
    throw new UnauthorizedException("You do not have access to this account.");
```
- `GetByIdWithClientAsync` eager-loads `Client` (`.Include(ba => ba.Client)`) and filters `!IsDeleted` — avoids a second query for the ownership check
- Ownership is anchored to `Client.CreatedByUserId` (the employee who registered the client), NOT the account's own `CreatedByUserId` — stays consistent even if a different employee later opens an account for that client
- Controller extracts `requestingUserId` from `ClaimTypes.NameIdentifier` and `isAdmin = User.IsInRole("Admin")`, passes both to the service

### Deposit / Withdraw
```csharp
public async Task<BankAccountResponseDto> DepositAsync(Guid id, decimal amount, Guid requestingUserId, bool isAdmin)
{
    var account = await _bankAccountRepository.GetByIdWithClientAsync(id)
        ?? throw new NotFoundException("BankAccount", id);

    if (!isAdmin && account.Client.CreatedByUserId != requestingUserId)
        throw new UnauthorizedException("You do not have access to this account.");

    if (account.Status != AccountStatus.Active)
        throw new ValidationException("Cannot deposit to a closed account.");

    account.Balance += amount;
    await _bankAccountRepository.UpdateAsync(account);
    await _bankAccountRepository.SaveChangesAsync();

    return BankAccountMapper.ToDto(account);
}
```
- `WithdrawAsync` mirrors this but subtracts, and additionally throws `ValidationException("Insufficient funds.")` if `account.Balance < amount`
- Both return the updated `BankAccountResponseDto` (200 OK) so the frontend can refresh the balance from the response or via query invalidation
- `[Range(0.01, double.MaxValue)]` on `TransactionDto.Amount` rejects `0`/negative amounts with 400 before the service is even called

### Soft delete (`DeleteAccountAsync`)
- `DeleteAsync(id)` in the repository sets `IsDeleted = true` and saves — never removes the row
- `GetByIdAsync` and `GetAllByClientIdAsync` both filter `!ba.IsDeleted`
- `GetByIdWithClientAsync` also filters `!IsDeleted`

### IBAN uniqueness
- Migration `AddBankAccountIbanUniqueIndex` adds a unique DB index on `BankAccounts.IBAN` — the authoritative guarantee
- `ExistsByIbanAsync(iban)` in the service gives a clean `409 Conflict` before hitting the DB constraint

### AccountStatus enum
```csharp
public enum AccountStatus
{
    Active,
    Closed
}
```
`BankAccountMapper.ToDto` serializes it as `Status.ToString()` (e.g. `"Active"`, `"Closed"`).

## Key details
- `GetAllByClientIdAsync` in the repository also `.Include(ba => ba.CreatedByUser)` — used for potential audit display
- `CloseAccountAsync` only flips `Status` to `Closed` — does NOT soft-delete; the account remains visible/listed
- Client role on `GET /api/clients/{clientId}/accounts`: controller checks `clientId == userId` from the JWT and returns `Forbid()` otherwise (clients can only see their own accounts)
- `DeleteAccount` (hard DELETE route, soft-delete impl) is Admin-only; `CloseAccount` is Employee+Admin

## Dependencies
- None beyond existing EF Core / Identity stack

## How to extend

### Add a new transaction type (e.g. Transfer)
1. Add `TransferAsync(fromId, toId, amount, requestingUserId, isAdmin)` to `IBankAccountService`/`BankAccountService`
2. Reuse `GetByIdWithClientAsync` + the same ownership check pattern for the source account
3. Add `[HttpPatch("~/api/accounts/{id}/transfer")]` to `BankAccountsController`, `[Authorize(Roles = "Employee,Admin")]`
4. Add DTO (e.g. `TransferDto { Amount, ToAccountId }`) in `DTOs/BankAccounts/`
5. Add unit tests (controller/service/repository) + integration tests following `BankAccountsIntegrationTests.cs` patterns (unique EGN/email/IBAN per test, client created by the acting employee)
