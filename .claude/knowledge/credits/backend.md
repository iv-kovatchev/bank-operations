# Credits Backend

## Overview
CRUD + annuity repayment plan generation for Consumer and Mortgage credits (TPT inheritance under `Credit`). Granting a credit always creates its `RepaymentPlan` + `RepaymentInstallments` in the same flow; updating a credit deletes and regenerates the plan from scratch. Employees can only operate on credits of clients they created; Admins bypass all ownership checks; Clients have read-only access to their own credits.

## Location
- `Controllers/CreditsController.cs` — REST endpoints (list/get/grant/update/repayment-plan)
- `Services/Credits/ICreditService.cs` + `CreditService.cs` — business logic, annuity calculation
- `Repositories/Credits/ICreditRepository.cs` + `CreditRepository.cs` — data access with eager loading
- `Mappers/Credits/CreditMapper.cs` — static mapper (also handles `CreditService` entity → DTO from the credit-services feature)
- `Entities/Credits/Credit.cs` (base, TPT) — `ConsumerCredit.cs`, `MortgageCredit.cs`
- `Entities/RepaymentPlan.cs`, `Entities/RepaymentInstallment.cs`
- `DTOs/Credits/ConsumerCredits/` — `CreateConsumerCreditDto`, `UpdateConsumerCreditDto`
- `DTOs/Credits/MortgageCredits/` — `CreateMortgageCreditDto`, `UpdateMortgageCreditDto`
- `DTOs/Credits/CreditResponseDto.cs`, `RepaymentPlanResponseDto.cs`, `RepaymentInstallmentResponseDto.cs`

## How it works

### Routes
`[Route("api/clients/{clientId}/credits")]` base + absolute overrides (`~/api/credits/...`), same pattern as `BankAccountsController`:
- `GET /api/clients/{clientId}/credits` — list (client-scoped; class-level `[Authorize(Roles = "Employee,Admin,Client")]`)
- `GET /api/credits/{id}` — get one
- `POST /api/clients/{clientId}/credits/consumer` — grant Consumer credit (Employee,Admin)
- `POST /api/clients/{clientId}/credits/mortgage` — grant Mortgage credit (Employee,Admin)
- `PUT /api/credits/{id}/consumer` — update Consumer credit (Employee,Admin)
- `PUT /api/credits/{id}/mortgage` — update Mortgage credit (Employee,Admin)
- `GET /api/credits/{id}/repayment-plan` — get the generated plan + installments

### Client self-access on GetAll
`CreditsController.GetAll` checks `User.IsInRole("Client")` and compares `clientId` to the JWT's `NameIdentifier`, returning `Forbid()` on mismatch — same pattern as `BankAccountsController`'s client check. Employee/Admin go straight to the service, which applies ownership filtering itself.

### Granting a credit (`GrantConsumerCreditAsync` / `GrantMortgageCreditAsync`)
1. Load `CreditService` (the credit product config) by `dto.CreditServiceId` → `NotFoundException` if missing
2. `ValidateCreditServiceLimits` — throws `ValidationException` if `Amount > MaxAmount` or `TermMonths > MaxTermMonths`
3. Load client via `_clientRepository.GetByIdWithDetailsAsync` (eager-loads `User`) → `ValidationException` if `!client.User.IsActive`
4. Create the `ConsumerCredit`/`MortgageCredit` entity with `Status = CreditStatus.Active`, save
5. `GenerateRepaymentPlan(credit, creditService)` — builds the annuity plan in memory, then `_context.RepaymentPlans.AddAsync(plan)` + a second `SaveChangesAsync()`
6. Two separate `SaveChangesAsync()` calls are needed: the credit needs a persisted `Id` before the plan (`RepaymentPlanId`/`CreditId` FK) can be safely built and saved

### Updating a credit (`UpdateConsumerCreditAsync` / `UpdateMortgageCreditAsync`)
1. `ValidateAndPrepareUpdateAsync<T>(id, requestingUserId, isAdmin) where T : Credit` — shared private helper:
   - `GetByIdWithDetailsAsync` → `NotFoundException("Credit", id)` if missing
   - cast to `T` → `NotFoundException(typeof(T).Name, id)` if wrong subtype
   - ownership check → `UnauthorizedException` if `!isAdmin && typedCredit.Client.CreatedByUserId != requestingUserId`
   - `typedCredit.Status != CreditStatus.Active` → `ValidationException("Cannot update a credit that is not active.")`
   - `typedCredit.RepaymentPlan?.Installments.Any(i => i.PaidAt != null) == true` → `ValidationException("Cannot update a credit with paid installments.")`
2. If `dto.CreditServiceId` changed, reload the new `CreditService` and re-run `ValidateCreditServiceLimits` against the new `Amount`/`TermMonths`
3. Assign the updated fields directly on the entity (`Purpose`/`Amount`/`TermMonths`/`CreditServiceId` for Consumer; `PropertyAddress`/`PropertyType`/... for Mortgage)
4. `DeleteAndRegenerateRepaymentPlan` — loads the existing plan with `.Include(rp => rp.Installments)`, removes installments + plan via `_context.RemoveRange`/`Remove`, then builds and adds a brand-new plan with `GenerateRepaymentPlan`
5. Single `SaveChangesAsync()` persists the entity changes + plan deletion + plan recreation together

### Annuity calculation (`GenerateRepaymentPlan`)
```csharp
decimal monthlyRate = creditService.InterestRate / 100 / 12;
decimal monthlyInstallment = credit.Amount *
    (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), credit.TermMonths)) /
    ((decimal)Math.Pow((double)(1 + monthlyRate), credit.TermMonths) - 1);
monthlyInstallment = Math.Round(monthlyInstallment, 2);
```
Then iterates `i = 1..TermMonths`, computing `interest = Round(remainingBalance * monthlyRate, 2)` and `principal = Round(monthlyInstallment - interest, 2)` per installment. **On the final installment** (`i == TermMonths`), `principal` is forced to equal `remainingBalance` exactly — this absorbs all rounding drift from the 35 (or however many) prior `Math.Round` calls so the loan balance reaches exactly zero instead of a few cents off.

### Repository eager loading
- `GetAllByClientIdAsync` — `.Include(c => c.Client).Include(c => c.CreditService)`
- `GetByIdWithDetailsAsync` — adds `.Include(c => c.RepaymentPlan).ThenInclude(rp => rp!.Installments.OrderBy(i => i.InstallmentNumber))` — needed so `RepaymentPlan?.Installments.Any(i => i.PaidAt != null)` in the update-validation helper works without a second query, and so installments come back in display order
- `GetRepaymentPlanAsync(creditId)` — standalone query loading a `RepaymentPlan` with its installments directly. Defined and unit-tested at the repository level, but **not currently called from `CreditService`** — the service's `GetRepaymentPlanAsync` instead reuses `credit.RepaymentPlan` already eager-loaded via `GetByIdWithDetailsAsync` (avoids a second query). Kept on the interface for symmetry/future direct use.

### CreditMapper
`CreditMapper.ToDto(Credit)` dispatches via switch expression to `ToDto(ConsumerCredit)`/`ToDto(MortgageCredit)`, each producing a flat `CreditResponseDto` with `CreditType` as a string (`"Consumer"`/`"Mortgage"`) and type-specific fields (`Purpose` vs `PropertyAddress`/`PropertyType`) left null on the other type. `ToDto(RepaymentPlan)` maps installments via a private `ToDto(RepaymentInstallment)` that computes `TotalAmount = PrincipalPart + InterestPart` and `IsPaid = PaidAt != null` in the DTO (matching the derived-value decision from `DECISIONS.md` — these are never stored).

## Key details

### CreditServiceEntity alias — required in any file touching both types
```csharp
using CreditServiceEntity = BankOperations.Entities.CreditService;
```
`CreditService` is both an entity (credit product config: interest rate, max amount, max term) and the service class implementing `ICreditService` for this very feature. Any file that needs both (the service class itself, and `CreditServiceTests.cs`) must alias the entity. Do not rename either class — this is the established pattern, also used in `Services/CreditServices/CreditServiceService.cs`.

### `DeleteAndRegenerateRepaymentPlan` bypasses the repository
This private helper queries `_context.RepaymentPlans` directly (via the injected `ApplicationDbContext`) instead of going through `ICreditRepository`, because it needs `.Include(rp => rp.Installments)` and `RemoveRange` on installments — operations not exposed on the repository interface. `CreditService` injects `ApplicationDbContext` directly alongside `ICreditRepository` for this reason (see constructor). This is a deliberate exception to "no DbContext outside repositories" for plan-regeneration only — keep new code going through the repository where possible.

### Ownership is anchored to `Client.CreatedByUserId`, not `Credit.CreatedByUserId`
Same pattern as bank accounts (see `.claude/knowledge/bank-accounts/backend.md`): `if (!isAdmin && credit.Client.CreatedByUserId != requestingUserId)`. This means ownership tracks who registered the *client*, not who granted the *credit* — consistent even if a different employee later grants a credit to that client.

### Update blocked on paid installments — MUST NOT be relaxed without a migration plan
`RepaymentInstallment.PaidAt != null` blocks `UpdateConsumerCreditAsync`/`UpdateMortgageCreditAsync` entirely (`ValidationException`). There is currently no partial-update or plan-amendment path for a credit with payment history — the only way to change `Amount`/`TermMonths` on such a credit would require a new feature (e.g. early-repayment / restructuring), not a fix to this check.

### Two `SaveChangesAsync()` calls in grant methods
Granting a credit calls `SaveChangesAsync()` once after `AddAsync(credit)` and again after `AddAsync(plan)`. `credit.Id` is already set in memory at plan-build time (`BaseEntity.Id` defaults to `Guid.NewGuid()` on construction), so `GenerateRepaymentPlan` can be called before the first save — but the plan row is only inserted after the credit row has actually been persisted, so the `RepaymentPlan.CreditId` FK always points at a row that exists in the DB.

### Generic update-validation helper
```csharp
private async Task<T> ValidateAndPrepareUpdateAsync<T>(Guid id, Guid requestingUserId, bool isAdmin) where T : Credit
{
    var credit = await _creditRepository.GetByIdWithDetailsAsync(id)
        ?? throw new NotFoundException("Credit", id);

    if (credit is not T typedCredit)
        throw new NotFoundException(typeof(T).Name, id);

    if (!isAdmin && typedCredit.Client.CreatedByUserId != requestingUserId)
        throw new UnauthorizedException("You do not have access to this credit.");

    if (typedCredit.Status != CreditStatus.Active)
        throw new ValidationException("Cannot update a credit that is not active.");

    if (typedCredit.RepaymentPlan?.Installments.Any(i => i.PaidAt != null) == true)
        throw new ValidationException("Cannot update a credit with paid installments.");

    return typedCredit;
}
```
Both update methods call this as `ValidateAndPrepareUpdateAsync<ConsumerCredit>(...)` / `<MortgageCredit>(...)`. Added during the docs pass on 2026-06-20 to eliminate duplicate validation that previously existed inline in both methods — see `DECISIONS.md` 2026-06-20 entry.

### `Client.ClientId`, not `Client.Id`
The `Client` entity's PK/FK-to-AspNetUsers property is named `ClientId` (not `Id` as an older example in `CONVENTIONS.md` shows). Test helpers and any new code referencing the client's identity must use `client.ClientId`.

## Code snippets

### Final-installment balance fix (annuity loop)
```csharp
for (int i = 1; i <= credit.TermMonths; i++)
{
    decimal interest = Math.Round(remainingBalance * monthlyRate, 2);
    decimal principal = Math.Round(monthlyInstallment - interest, 2);

    if (i == credit.TermMonths)
        principal = remainingBalance; // absorb rounding drift on the last installment

    remainingBalance = Math.Round(remainingBalance - principal, 2);
    // ... build RepaymentInstallment
}
```

### Plan deletion + regeneration on update
```csharp
private async Task DeleteAndRegenerateRepaymentPlan(Credit credit, CreditServiceEntity creditService)
{
    var existingPlan = await _context.RepaymentPlans
        .Include(rp => rp.Installments)
        .FirstOrDefaultAsync(rp => rp.CreditId == credit.Id);
    if (existingPlan != null)
    {
        _context.RepaymentInstallments.RemoveRange(existingPlan.Installments);
        _context.RepaymentPlans.Remove(existingPlan);
    }

    var plan = GenerateRepaymentPlan(credit, creditService);
    await _context.RepaymentPlans.AddAsync(plan);
}
```

## Dependencies
- None beyond the existing EF Core / Identity stack. Reuses `IClientRepository.GetByIdWithDetailsAsync` (from the bank-accounts/clients features) for the active-client check on credit grant.

## Tests
- `BankOperations.Tests/Unit/Controllers/Credits/CreditsControllerTests.cs` — 10 tests
- `BankOperations.Tests/Unit/Services/Credits/CreditServiceTests.cs` — 21 tests (annuity math, limit validation, ownership, paid-installment block, plan regeneration)
- `BankOperations.Tests/Unit/Repositories/Credits/CreditRepositoryTests.cs` — 12 tests
- `BankOperations.Tests/Integration/Credits/CreditsIntegrationTests.cs` — 11 tests (full HTTP pipeline, role/ownership checks)
- Total project: 190 tests passing (136 before this feature + 54 new)
- Service-level tests construct a real `ApplicationDbContext` backed by `UseInMemoryDatabase(Guid.NewGuid().ToString())` (not a mock) because `CreditService` needs a concrete `DbContext` for `DeleteAndRegenerateRepaymentPlan`'s direct `_context.RepaymentPlans`/`RepaymentInstallments` access — mocking `ICreditRepository`/`ICreditServiceRepository`/`IClientRepository` alone isn't enough.

## How to extend

### Add a new credit type
1. Create entity inheriting from `Credit` in `Entities/Credits/`, add TPT config in `Data/Configurations/`
2. Add `CreateXDto`/`UpdateXDto` in `DTOs/Credits/[NewType]/`
3. Add `ToDto(XCredit)` overload in `CreditMapper` + a case in the base `ToDto(Credit)` switch
4. Add `GrantXCreditAsync`/`UpdateXCreditAsync` to `ICreditService`/`CreditService`, reusing `ValidateAndPrepareUpdateAsync<XCredit>` for the update path
5. Add endpoints to `CreditsController` (`POST .../credits/x`, `PUT ~/api/credits/{id}/x`)
6. Add unit + integration tests following the Consumer/Mortgage pattern

### Add early repayment / restructuring
Would need a new service method that bypasses the "no paid installments" block in `ValidateAndPrepareUpdateAsync` — do not weaken the existing check; add a separate, explicit operation instead (e.g. `RestructureCreditAsync`) that knows how to reconcile already-paid installments against a new plan.
