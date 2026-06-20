# Installments — Pay / Unpay

## Overview
Marks a single `RepaymentInstallment` on a credit's repayment plan as paid or unpaid. Paying an installment withdraws its total (`PrincipalPart + InterestPart`) from a bank account the employee selects, and automatically flips `Credit.Status` to `PaidOff` once every installment on the plan is paid. Unpaying reverses both the installment state and the `PaidOff`→`Active` status (no money is credited back — see Key details).

## Location
- `Controllers/CreditsController.cs` — `PATCH ~/api/credits/{creditId}/installments/{installmentId}/pay`, `PATCH ~/api/credits/{creditId}/installments/{installmentId}/unpay`; both `[Authorize(Roles = "Employee,Admin")]`
- `Services/Credits/ICreditService.cs` / `CreditService.cs` — `PayInstallmentAsync(creditId, installmentId, bankAccountId, requestingUserId, isAdmin)`, `UnpayInstallmentAsync(creditId, installmentId, requestingUserId, isAdmin)`
- `Repositories/Credits/ICreditRepository.cs` / `CreditRepository.cs` — `GetInstallmentByIdAsync(Guid installmentId)` (eager-loads `RepaymentPlan`; defined for symmetry, not currently called by `CreditService` — see Key details)
- `DTOs/Credits/PayInstallmentDto.cs` — `BankAccountId` (`Guid`, `[Required]`)
- `Mappers/Credits/CreditMapper.cs` — `ToDto(RepaymentInstallment)` is `public` (was `private` before this feature) so `CreditService` can map the mutated installment directly
- `frontend/src/api/credits/usePayInstallment.ts` / `useUnpayInstallment.ts` — mutation hooks
- `frontend/src/pages/Employee/Clients/components/PayInstallmentForm/` — `PayInstallmentForm.tsx` + `usePayInstallmentForm.ts` + `payInstallmentForm.schema.ts`
- `frontend/src/pages/Employee/Clients/components/RepaymentPlanSection/` — Pay/Unpay buttons + `payModal` state

## How it works

### PayInstallmentAsync
1. `GetByIdWithDetailsAsync(creditId)` — loads the credit with `Client`, `CreditService`, and `RepaymentPlan.Installments` eager-loaded → `NotFoundException` if missing
2. Ownership check: `!isAdmin && credit.Client.CreatedByUserId != requestingUserId` → `UnauthorizedException`
3. `credit.Status != CreditStatus.Active` → `ValidationException` (can't pay on a non-active credit)
4. Find the installment in `credit.RepaymentPlan.Installments` by id → `NotFoundException` if missing
5. `installment.PaidAt != null` → `ValidationException("This installment is already paid.")`
6. Set `installment.PaidAt = DateTime.UtcNow` and `installment.CreatedByUserId = requestingUserId`
7. Load the bank account via `_bankAccountRepository.GetByIdWithClientAsync(bankAccountId)` → `NotFoundException` if missing
8. `bankAccount.Status != AccountStatus.Active` → `ValidationException` (can't pay from a closed account)
9. `bankAccount.Balance < installmentTotal` (where `installmentTotal = PrincipalPart + InterestPart`) → `ValidationException("Insufficient funds.")`
10. Debit the account: `bankAccount.Balance -= installmentTotal`, then `_bankAccountRepository.UpdateAsync(bankAccount)`
11. If `credit.RepaymentPlan.Installments.All(i => i.PaidAt != null)` → `credit.Status = CreditStatus.PaidOff`
12. Single `_creditRepository.SaveChangesAsync()` persists the installment, the credit status, **and** the bank account balance change together

### UnpayInstallmentAsync
1. Same load + ownership check as Pay (no active-status check — you need to be able to unpay a `PaidOff` credit to bring it back)
2. Find installment → `NotFoundException` if missing
3. `installment.PaidAt == null` → `ValidationException("This installment is not paid.")`
4. Clear `installment.PaidAt = null` and `installment.CreatedByUserId = null`
5. If `credit.Status == CreditStatus.PaidOff` → revert to `CreditStatus.Active`
6. `SaveChangesAsync()`

### Frontend flow
- "Pay" button (shown when `!installment.isPaid && role !== 'Client'`) opens a `payModal` state in `useRepaymentPlanSection` instead of firing the mutation directly — it renders `PayInstallmentForm` in a `FormModal`
- `usePayInstallmentForm` loads the client's accounts via `useGetClientAccounts(clientId)`, filters to `AccountStatus.Active`, and renders them in a `Select` as `{iban} — {balance.toFixed(2)} BGN`
- "Unpay" button (shown when `installment.isPaid && role !== 'Client'`) calls `useUnpayInstallment` directly — no form needed since there's no account to pick

## Key details

### Single `SaveChangesAsync()` persists both repositories' changes
`_bankAccountRepository.UpdateAsync(bankAccount)` only calls `_context.BankAccounts.Update(entity)` — it does **not** save. `CreditService` injects both `ICreditRepository` and `IBankAccountRepository`, which share the same scoped `ApplicationDbContext` per request. Calling `_creditRepository.SaveChangesAsync()` once at the end of `PayInstallmentAsync` persists the installment mutation, the `Credit.Status` change, and the bank account debit atomically in one transaction. This is the same pattern already used for plan regeneration (see `.claude/knowledge/credits/backend.md`).

### `IBankAccountRepository` injected directly — not `IBankAccountService`
`CreditService`'s constructor takes `IBankAccountRepository`, not `IBankAccountService`. The repository's `GetByIdWithClientAsync` and `UpdateAsync` are what's needed; going through `IBankAccountService` would mean re-implementing or duplicating its deposit/withdraw validation (ownership, active-status, balance checks) instead of reusing the already-loaded `credit`/`installment` context. `IBankAccountRepository` is already registered in `Config/RepositoryExtensions.cs`, so no new DI registration was needed.

### Unpay does not refund the bank account
`UnpayInstallmentAsync` clears `PaidAt` but never touches `bankAccount.Balance`. Unpaying is a correction/undo of a data-entry mistake, not a real-world reversal of a bank transaction — if real money needs to move back, that must be done as an explicit deposit via the existing account-transactions feature.

### `installment.TotalAmount` does not exist on the entity
The installment total is computed inline as `installment.PrincipalPart + installment.InterestPart` in `CreditService`. `RepaymentInstallment` (the entity) intentionally has no `TotalAmount` property — it's a derived value that only exists on `RepaymentInstallmentResponseDto` (see `DECISIONS.md` 2026-05-15 entry on derived values). Don't add a `TotalAmount` property to the entity to "simplify" this — compute it at the call site instead.

### Cache invalidation differs between Pay and Unpay
- `usePayInstallment` invalidates `['repayment-plan', creditId]`, `['credits', clientId]`, **and** `['accounts', clientId]` — the bank account balance changed, so `AccountsSection` must refetch too
- `useUnpayInstallment` invalidates only `['repayment-plan', creditId]` and `['credits', clientId]` — no account balance changes on unpay, so no accounts invalidation is needed

### `GetInstallmentByIdAsync` is unused by `CreditService`
The repository method exists on `ICreditRepository`/`CreditRepository` but `PayInstallmentAsync`/`UnpayInstallmentAsync` find the installment via the already-loaded `credit.RepaymentPlan.Installments` (from `GetByIdWithDetailsAsync`) instead of a second query. Kept on the interface for symmetry/future direct use, same rationale as `GetRepaymentPlanAsync` (see `.claude/knowledge/credits/backend.md`).

### `[Required]` on a non-nullable `Guid` is effectively a no-op
`PayInstallmentDto.BankAccountId` is `Guid` (not `Guid?`) with `[Required]`. ASP.NET's `RequiredAttribute` only fails validation on `null`; a non-nullable `Guid` can never be `null`, so an empty/default `Guid` would still pass model validation and instead surface as a `NotFoundException` from `GetByIdWithClientAsync` inside the service. The attribute is kept for documentation/intent, not enforcement.

## Code snippets

### Debit + status flip (PayInstallmentAsync, abbreviated)
```csharp
installment.PaidAt = DateTime.UtcNow;
installment.CreatedByUserId = requestingUserId;

var bankAccount = await _bankAccountRepository.GetByIdWithClientAsync(bankAccountId)
    ?? throw new NotFoundException("BankAccount", bankAccountId);

if (bankAccount.Status != AccountStatus.Active)
    throw new ValidationException("Cannot pay an installment from a closed account.");

decimal installmentTotal = installment.PrincipalPart + installment.InterestPart;

if (bankAccount.Balance < installmentTotal)
    throw new ValidationException("Insufficient funds.");

bankAccount.Balance -= installmentTotal;
await _bankAccountRepository.UpdateAsync(bankAccount);

if (credit.RepaymentPlan!.Installments.All(i => i.PaidAt != null))
    credit.Status = CreditStatus.PaidOff;

await _creditRepository.SaveChangesAsync(); // persists installment + credit + bank account together
```

### Frontend — Pay opens a form, Unpay fires directly
```ts
// useRepaymentPlanSection.ts
const handlePayClick = (installmentId: string) => {
  if (role === 'Client') return;
  setPayModal({ open: true, installmentId }); // opens FormModal → PayInstallmentForm
};

const handleUnpayClick = (installmentId: string) => {
  if (role === 'Client') return;
  unpayInstallment({ creditId, installmentId, clientId }); // direct mutation, no form
};
```

## Dependencies
- None beyond the existing EF Core stack. Reuses `IBankAccountRepository` from the bank-accounts feature.

## How to extend

### Refund the account on unpay
Add the account-debit reversal symmetrically: inject `IBankAccountRepository` usage into `UnpayInstallmentAsync`, but the installment doesn't currently store which `BankAccountId` was used to pay it — `RepaymentInstallment` has no such column. Add a `PaidFromBankAccountId` (nullable `Guid`) to the entity first if a real refund-on-unpay is needed.

### Add tests
This feature shipped without unit/integration tests (see `PROGRESS.md` note). Follow the existing `CreditServiceTests.cs` pattern: mock `ICreditRepository`, `ICreditServiceRepository`, `IClientRepository`, and the new `IBankAccountRepository`; cover both happy paths and each `ValidationException`/`NotFoundException`/`UnauthorizedException` branch for `PayInstallmentAsync`/`UnpayInstallmentAsync`.
