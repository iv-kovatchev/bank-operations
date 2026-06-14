# Bank Accounts Frontend

## Overview
Accounts section on the client detail page: list accounts, open a new account, close/delete an account, and deposit/withdraw funds. Employees and Admins see action buttons; Clients see read-only data.

## Location
- `src/types/bank-account.types.ts` — `AccountStatus`, `BankAccountResponse`, `CreateBankAccountDto`, `TransactionDto`
- `src/api/bank-accounts/` — one hook per file: `useGetClientAccounts`, `useOpenAccount`, `useCloseAccount`, `useDeleteAccount`, `useDepositToAccount`, `useWithdrawFromAccount`
- `src/pages/Employee/Clients/components/AccountsSection/AccountsSection.tsx` — render-only component
- `src/pages/Employee/Clients/components/AccountsSection/useAccountsSection.ts` — all state/handlers/data
- `src/pages/Employee/Clients/components/OpenAccountForm/OpenAccountForm.tsx` + `useOpenAccountForm` pattern (open account form)
- `src/pages/Employee/Clients/components/TransactionForm/TransactionForm.tsx` — render-only deposit/withdraw form
- `src/pages/Employee/Clients/components/TransactionForm/useTransactionForm.ts` — form/mutation logic

## How it works

### Types (`bank-account.types.ts`)
```ts
export const AccountStatus = { Active: 'Active', Closed: 'Closed' } as const;
export type AccountStatus = typeof AccountStatus[keyof typeof AccountStatus];

export interface BankAccountResponse {
  id: string;
  iban: string;
  balance: number;
  status: AccountStatus;
  clientId: string;
  createdAt: string;
  createdByUserId: string;
}

export interface CreateBankAccountDto { iban: string; initialBalance: number; }
export interface TransactionDto { amount: number; }
```

### API hooks (`src/api/bank-accounts/`)
All mutation hooks invalidate `['accounts', clientId]` on success — same pattern as `useCloseAccount`/`useOpenAccount`:
```ts
type DepositVariables = { id: string; clientId: string; amount: number };

export const useDepositToAccount = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, amount }: DepositVariables) => {
      const dto: TransactionDto = { amount };
      return http.patch<BankAccountResponse>(`/api/accounts/${id}/deposit`, dto);
    },
    onSuccess: (_data, { clientId }) => {
      queryClient.invalidateQueries({ queryKey: ['accounts', clientId] });
    },
  });
};
```
`useWithdrawFromAccount` is identical but calls `/api/accounts/${id}/withdraw`.

**`http.patch<T>` takes a single generic type parameter** (`patch = async <T>(url, body?: unknown): Promise<T>`) — do not pass a second type argument for the body type. Type the body as a local `const dto: TransactionDto = { ... }` instead.

### AccountsSection — component + co-located hook
`AccountsSection.tsx` is render-only; all state and handlers live in `useAccountsSection(clientId)`:
- State: `isOpenAccountModalOpen`, `confirmClose`/`confirmDelete` (`{ open, accountId }`), `transactionModal` (`{ open, accountId, mode: 'deposit' | 'withdraw' | null }`)
- Handlers: `handleOpenAccountModal/handleCloseAccountModal`, `handleCloseAccountClick/handleCancelClose/handleConfirmClose`, `handleDeleteClick/handleCancelDelete/handleConfirmDelete`, `handleDepositClick/handleWithdrawClick/handleCloseTransaction`
- Returns `accounts`, `isLoading`, `isClosing`, `isDeleting`, `isDepositing`, `isWithdrawing` + all state/handlers
- Confirm-based actions (Close/Delete) go through `ConfirmModal`; Deposit/Withdraw go directly to a `FormModal` rendering `TransactionForm`

### Table action buttons (role + status gated)
```tsx
{role !== 'Client' && account.status === AccountStatus.Active && (
  <Button variant="soft" color="green" size="1" onClick={() => handleDepositClick(account.id)}>Deposit</Button>
)}
{role !== 'Client' && account.status === AccountStatus.Active && (
  <Button variant="soft" color="amber" size="1" onClick={() => handleWithdrawClick(account.id)}>Withdraw</Button>
)}
{(role === 'Admin' || role === 'Employee') && account.status === AccountStatus.Active && (
  <Button variant="soft" color="red" size="1" onClick={() => handleCloseAccountClick(account.id)}>Close</Button>
)}
{role === 'Admin' && account.status === AccountStatus.Closed && (
  <Button variant="soft" color="red" size="1" onClick={() => handleDeleteClick(account.id)}>Delete</Button>
)}
```

### TransactionForm — shared deposit/withdraw form
Component + co-located hook pattern (`TransactionForm/TransactionForm.tsx` + `TransactionForm/useTransactionForm.ts`), same as `OpenAccountForm`:
```tsx
<FormModal
  open={transactionModal.open}
  title={transactionModal.mode === 'deposit' ? 'Deposit' : 'Withdraw'}
  onClose={handleCloseTransaction}
>
  {transactionModal.accountId && transactionModal.mode && (
    <TransactionForm
      accountId={transactionModal.accountId}
      clientId={clientId}
      mode={transactionModal.mode}
      onClose={handleCloseTransaction}
    />
  )}
</FormModal>
```

`useTransactionForm` selects the mutation hook based on `mode`, builds the Zod schema with `z.preprocess`, and exposes `register, handleSubmit, errors, isPending, error, onSubmit`:
```ts
const transactionSchema = z.object({
  amount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number({ error: 'Amount is required' }).min(0.01, 'Amount must be greater than 0')
  ),
});

type TransactionFormInput = z.input<typeof transactionSchema>;   // { amount: unknown }
type TransactionFormOutput = z.output<typeof transactionSchema>; // { amount: number }

export const useTransactionForm = ({ mode, accountId, clientId, onClose }: UseTransactionFormParams) => {
  const deposit = useDepositToAccount();
  const withdraw = useWithdrawFromAccount();
  const { mutate, isPending, error } = mode === 'deposit' ? deposit : withdraw;

  const { register, handleSubmit, formState: { errors } } =
    useForm<TransactionFormInput, unknown, TransactionFormOutput>({
      resolver: zodResolver(transactionSchema),
      defaultValues: { amount: 0 },
    });

  const onSubmit = (data: TransactionFormOutput) => {
    mutate({ id: accountId, clientId, amount: data.amount }, { onSuccess: onClose });
  };

  return { register, handleSubmit, errors, isPending, error, onSubmit };
};
```

`TransactionForm.tsx` renders a single "Amount (BGN)" `TextField.Root` (`type="number" step="0.01"`), a `Toast` for `error`, Cancel button (`onClose`), and a submit `Button` labeled `"Deposit"` or `"Withdraw"` based on `mode`.

## Key details

### Zod `z.preprocess` for numeric inputs — required for correct error messages
A plain `z.number()` schema on an HTML number input produces `NaN` (not `undefined`) when the field is empty, which Zod reports as "Expected number, received nan" — not useful to the user. `z.preprocess` normalizes `''`/`null`/`undefined` to `undefined` first, so `z.number({ error: 'Amount is required' })` fires the intended message on an empty field.

### Zod v4 — `error` not `required_error`/`message`
This project uses **Zod v4.4.3**. The v3 API `z.number({ required_error: '...' })` does not exist in v4 — use `z.number({ error: '...' })` instead.

### 3-generic `useForm` required when schema input/output types differ
`z.preprocess` makes the schema's **input** type `unknown` but its **output** type `number`. `zodResolver`'s inferred `Resolver` type then has `TFieldValues = { amount: unknown }`, which is NOT assignable to a single-generic `useForm<{ amount: number }>()`. Fix: use react-hook-form's 3-generic form —
```ts
useForm<z.input<typeof schema>, unknown, z.output<typeof schema>>({ resolver: zodResolver(schema) })
```
`onSubmit`'s `data` parameter is then typed as `z.output<typeof schema>` (`{ amount: number }`), while `register('amount', ...)` still works against the input type (`{ amount: unknown }`).

## Dependencies
- `zod` 4.4.3, `@hookform/resolvers` 5.4.0, `react-hook-form` 7.76.1

## How to extend

### Add a new transaction type (e.g. Transfer)
1. Add `useTransferBetweenAccounts.ts` to `src/api/bank-accounts/` following the deposit/withdraw pattern (invalidate `['accounts', clientId]` for both accounts involved)
2. Extend `TransactionForm`'s `mode` union or create a new sibling form component + hook in its own folder
3. Add a new button + modal wiring in `useAccountsSection`/`AccountsSection`
