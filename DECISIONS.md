# DECISIONS.md — Architectural Decisions

> Log every significant architectural decision here with date and reasoning.
> This file is critical for project defense — it explains WHY things are built the way they are.

---

## 2025 — Initial Architecture

### Single-project backend structure
**Decision:** All backend code lives in one ASP.NET project (`BankOperations/`) with feature folders, instead of a layered multi-project solution.
**Why:** Simpler to navigate and maintain for a solo developer. Avoids over-engineering while still maintaining clean separation via folders.

### Generic interfaces only — no generic implementations
**Decision:** `IGenericRepository<T>` and `IGenericService<T>` exist as base interfaces, but each repository and service writes its own full implementation.
**Why:** Each entity has specific logic (e.g. find by EGN, generate repayment plan). A generic base class would constrain implementations. Interfaces enforce a consistent contract without limiting flexibility.

### TPT (Table Per Type) inheritance
**Decision:** Used TPT for Clients (IndividualClient, CorporateClient) and Credits (ConsumerCredit, MortgageCredit).
**Why:** The assignment explicitly requires inheritance. TPT creates a separate table per subclass with a 1:1 FK to the base table, making the inheritance clearly visible in the DB schema. TPC would duplicate columns; TPH would use nulls.

### JWT + Refresh Token authentication
**Decision:** Access Token (15 min lifetime) + Refresh Token stored in HttpOnly cookie.
**Why:** Standard secure approach for REST API + SPA architecture. Short-lived access token limits damage if intercepted. HttpOnly cookie prevents JS access to refresh token (XSS protection).

### Two-Factor Authentication (2FA)
**Decision:** Implement 2FA with OTP code sent via email on every login.
**Why:** Banking system requires higher security. If password is stolen, attacker still cannot login without access to the email. ASP.NET Identity has built-in 2FA support making implementation straightforward (~1-2 extra days of work).

### CreditServices table
**Decision:** Interest rate, max amount, and max term are stored in a `CreditServices` table, not hardcoded.
**Why:** The assignment states these values are determined by credit type. Storing them in DB allows changes without redeployment.

### Custom Exceptions
**Decision:** Custom exception classes (NotFoundException, ConflictException, ValidationException) caught by global middleware.
**Why:** Clean separation — services throw typed exceptions, middleware handles HTTP response mapping. Controllers stay thin and don't need try/catch blocks.

### ActivityLogs table
**Decision:** Every employee action is recorded in an `ActivityLogs` table.
**Why:** Admin requirement — admins must be able to see who did what and when. Implemented as a service called from every operation.

### Radix UI Themes for frontend
**Decision:** `@radix-ui/themes` instead of Chakra UI, Tailwind CSS, or MUI.
**Why:** Radix Themes provides a complete, accessible design system with dark mode, custom color palettes (including P3 wide-gamut), and composable layout primitives. It is framework-agnostic and integrates cleanly with Vite + React 19. Chakra UI has React 19 compatibility issues at the time of setup.

### React Query for server state
**Decision:** React Query for all server data fetching and caching.
**Why:** Handles loading/error states, caching, and refetching automatically. Eliminates boilerplate compared to manual useEffect + useState data fetching.

### CI/CD directly from develop to Azure
**Decision:** GitHub Actions deploys automatically on every push to `develop`. No `main` branch.
**Why:** Faster feedback loop — every merged feature is immediately visible in production. Simpler branching strategy for solo development.

---

## 2026-05-13 — backend-setup

### .NET 10 instead of .NET 8
**Decision:** Target framework is `net10.0`.
**Why:** .NET 10 is the version installed on the dev machine. No reason to downgrade.

### Swagger only in Development
**Decision:** `app.UseSwagger()` and `app.UseSwaggerUI()` are wrapped in `if (app.Environment.IsDevelopment())`.
**Why:** Swagger exposes the full API surface. Keeping it off in production (Azure) reduces the attack surface and avoids leaking internal endpoint structure.

---

## 2026-05-14 — ci-cd

### Azure OIDC Federated Identity instead of client secret
**Decision:** The GitHub Actions workflow authenticates to Azure using OIDC Federated Identity (`azure/login@v2` with `client-id`, `tenant-id`, `subscription-id` secrets) rather than a long-lived client secret or publish profile.
**Why:** Federated credentials are short-lived tokens issued per workflow run — no secret rotation needed and no risk of a leaked long-lived credential. The `id-token: write` permission is scoped only to the deploy job, not the build job.

### Path filter on CI trigger
**Decision:** The workflow triggers only when files under `backend/**` are changed.
**Why:** Frontend changes, docs updates, and configuration files should not trigger a backend deployment. Keeps pipeline runs fast and avoids unnecessary Azure deployments for unrelated commits.

### Two-job pipeline (build + deploy)
**Decision:** Build and deploy are split into separate jobs with artifact hand-off via `actions/upload-artifact` / `actions/download-artifact`.
**Why:** Standard GitHub Actions pattern — isolates the build environment from the deploy environment, allows the deploy job to be re-run independently if a deploy fails without rebuilding, and makes permissions minimal per job (`contents: read` on build, `id-token: write` on deploy).

---

## 2026-05-15 — Domain model revisions

### Three roles: Admin, Employee, Client
**Decision:** `AspNetUsers` has three roles: Admin, Employee, and Client. Clients are users with role `Client` and read-only access to their own data.
**Why:** The original design excluded clients from the system entirely. Adding a Client role with JWT auth allows the same API to serve client-facing endpoints without a separate application. Role-based authorization (`[Authorize(Roles = "...")]`) keeps the permission model simple and explicit.

### Client registration in one step (Employee/Admin creates AspNetUsers + Client record atomically)
**Decision:** When an Employee or Admin registers a new client, a single service call creates the `AspNetUsers` account (role=Client), the `Client`/`IndividualClient` or `CorporateClient` record, and sends a welcome email with the generated password — all in one transaction.
**Why:** A two-step flow (create user, then create client) would allow an inconsistent state where a client user exists without a Client record, or vice versa. A single atomic operation prevents this. It also reduces the UI to one form, which is better UX for employees.

### Clients table: ClientId is PK and FK → AspNetUsers (1:1); Status and CreatedAt removed
**Decision:** `Clients.ClientId` is both the primary key and a foreign key pointing to `AspNetUsers.Id`. `Status` and `CreatedAt` columns are removed from the `Clients` table.
**Why:** Every client IS an AspNetUsers account — there is no meaningful difference between `Client.Id` and `ApplicationUser.Id`. Using the user's Id as the PK eliminates a redundant column and enforces the 1:1 constraint at the database level. `IsActive` and `CreatedAt` already exist on `AspNetUsers` — duplicating them in `Clients` would create two sources of truth.

### RepaymentInstallments: TotalAmount and IsPaid removed (derived values)
**Decision:** `TotalAmount` and `IsPaid` columns are removed from `RepaymentInstallments`. `TotalAmount` is computed as `PrincipalPart + InterestPart`; `IsPaid` is determined by `PaidAt != null`.
**Why:** Storing values that can be derived from other columns risks inconsistency — if `PrincipalPart` is ever corrected, `TotalAmount` could silently become stale. Computed properties on the entity class are sufficient and always consistent.

---

## 2026-05-28 — feature/auth

### Startup seeding via DataSeeder
**Decision:** Roles and the initial admin account are seeded at application startup inside `Program.cs` using `app.Services.CreateScope()`, not via a migration or a one-off script.
**Why:** Migrations run in CI before the app boots and have no access to `UserManager` / `RoleManager`. A startup seeder runs in the full DI context, making it the only practical place to use Identity APIs. All seed operations are idempotent (existence-checked before insert), so re-running on every startup is safe with no performance penalty beyond a few DB reads.

### Two-step login flow (password → OTP → tokens)
**Decision:** Login is split into two HTTP calls: `POST /api/auth/login` validates credentials and emails an OTP; `POST /api/auth/verify-otp` validates the OTP and issues the JWT access token + refresh token.
**Why:** Mandatory 2FA on every login. Combining both steps in one call would require issuing tokens before OTP confirmation, which defeats the purpose of 2FA. Splitting them allows the frontend to show an OTP entry screen without holding any credentials in memory between steps.

### Refresh token delivered via HttpOnly cookie only — `[JsonIgnore]` on DTO
**Decision:** `AuthResultDto.RefreshToken` is annotated `[JsonIgnore]`. The refresh token is set as an HttpOnly cookie in `AuthController.VerifyOtpAsync` and never appears in the response body.
**Why:** A refresh token in the response body is accessible to JavaScript, making it vulnerable to XSS. An HttpOnly cookie is invisible to JS. The DTO field still exists so the service layer can pass the value up to the controller cleanly without breaking the layer boundary.

### OTP invalidated before generating a new one
**Decision:** `OtpService.GenerateAndSaveOtpAsync` calls `InvalidateAllForUserAsync` before saving the new OTP.
**Why:** Without this, a user who requests a second OTP still has their first (valid) OTP in the database. An attacker who intercepted the first code could use it even after a second send. Invalidating all previous codes on each new request closes this window.

### SmtpClient instead of SendGrid
**Decision:** `EmailService` uses `System.Net.Mail.SmtpClient` with SMTP credentials from env vars, not the SendGrid SDK.
**Why:** The project is in early development and SendGrid requires account setup, API key management, and an external dependency. `SmtpClient` works with any SMTP provider (Gmail, Outlook, etc.) and has zero dependencies. Can be swapped for SendGrid later by replacing `EmailService` behind `IEmailService` without touching any other code.

### DI registrations extracted to extension methods in `Config/`
**Decision:** All `builder.Services.AddScoped<...>()` calls for repositories and services live in `Config/RepositoryExtensions.cs` and `Config/ServiceExtensions.cs`, invoked from `Program.cs` as `builder.Services.AddRepositories()` and `builder.Services.AddServices()`.
**Why:** `Program.cs` grows long quickly as features are added. Grouping registrations by layer in extension methods keeps `Program.cs` readable and avoids merge conflicts when multiple features add registrations at the same time.

---

## 2026-05-28 — feature/frontend-setup

### Native fetch wrapper instead of Axios
**Decision:** HTTP calls use a thin `src/services/http.ts` wrapper around the native `fetch` API instead of Axios.
**Why:** Axios adds ~14 KB and no meaningful benefit when `fetch` is universally supported. The wrapper handles JWT injection, 401 redirect, and error parsing in ~50 lines, covering all project needs without a dependency.

### JWT role claim as plain `"role"` string
**Decision:** `TokenService.cs` emits the role claim with key `"role"` instead of `ClaimTypes.Role` (which expands to the long Microsoft schema URI).
**Why:** `ClaimTypes.Role` produces `"http://schemas.microsoft.com/ws/2008/06/identity/claims/role"` as the JWT key, requiring complex decoding on the frontend. A plain `"role"` key is readable, standard (matches OAuth2/OIDC conventions), and works with `jwtDecode<{ role: string }>()` directly. `TokenValidationParameters.RoleClaimType = "role"` must be set in `Program.cs` to keep `[Authorize(Roles)]` working on the backend.

### Auth hooks call http directly — no intermediate service layer
**Decision:** React Query mutation hooks in `src/api/auth/` call `http.post()` directly; there is no separate `authService.ts` or `authApi.ts` object between the hook and the HTTP layer.
**Why:** An intermediate service layer adds a file and a function call with no benefit for simple CRUD mutations. The hook already encapsulates the mutation logic (`onSuccess`, `onError`, navigation). Adding a service layer would split logic that belongs together across two files.

### Each React Query hook in its own file
**Decision:** Auth hooks are split into `useLogin.ts`, `useVerifyOtp.ts`, `useLogout.ts` — one file per hook — rather than a single `authApi.queries.ts`.
**Why:** A single queries file becomes a growing list of unrelated exports. Individual files are easier to locate, import selectively, and review in isolation. The pattern scales to other domains (clients, accounts, credits) without producing large barrel files.

### No inline styles — CSS files or Radix props only
**Decision:** `style={{ }}` inline props are banned. Styling must use Radix UI component props (layout, color, size, spacing) or CSS classes defined in co-located `.styles.css` files.
**Why:** Inline styles bypass the Radix CSS variable system (dark mode, theming), are not reusable, and mix presentation with structure. Co-located `.styles.css` files keep styles close to the component without polluting JSX.

### Shared types in `src/types/`
**Decision:** TypeScript types shared across multiple files live in `src/types/` (e.g. `auth.types.ts`), not co-located with the API hook files that use them.
**Why:** Types are referenced by hooks, components, and pages. Placing them in `src/api/auth/` would create import paths like `../../api/auth/authApi.types` from a component — coupling the component to the API layer's folder structure. `src/types/` is a neutral location accessible from anywhere.

---

## 2026-05-29 — feature/frontend-auth

### Context split: definition file + provider file
**Decision:** Each context is split into two files: `*ContextDef.ts` (exports the context object and its type, no JSX) and `*Context.tsx` (exports only the provider component).
**Why:** Vite Fast Refresh requires that a `.tsx` file exports only React components. Exporting both `AuthContext` (a non-component value) and `AuthContextProvider` (a component) from the same file breaks HMR. Separating them satisfies the rule without changing the public API — consumers import the hook from `useAuth.ts` and the provider from `AuthContext.tsx`.

### Proactive token refresh via React Query instead of reactive 401 retry
**Decision:** Access token is refreshed proactively every 14 minutes using `useQuery` inside `AuthContextProvider`, not by intercepting 401 responses in `http.ts`.
**Why:** A reactive approach (catch 401 → refresh → retry) requires queueing concurrent failed requests and retrying them, which adds significant complexity to the HTTP layer. A proactive approach with a 14-minute interval (token expires in 15 minutes) keeps `http.ts` simple and eliminates the retry entirely. The `queryFn` stores the new token in `localStorage` directly (external system sync — correct place), so no `setState` is called in a `useEffect`, avoiding the cascading render lint warning.

### Route-based single-point layout
**Decision:** `PageLayout` and the public header shell are applied via nested layout routes (`AuthenticatedLayout`, `PublicLayout`) defined once in `src/routes/index.tsx`, not imported in individual page components.
**Why:** Importing `PageLayout` in every page creates repetitive boilerplate and a risk of pages accidentally missing the layout. A single layout route wraps all pages in a group, making the layout implicit and guaranteed. Adding a new authenticated page only requires adding a `<Route>` — no layout import needed.

### ThemeContext owns Radix appearance
**Decision:** `ThemeContextProvider` manages `'light' | 'dark'` state. `App.tsx` reads from `useTheme()` and passes it to the Radix `<Theme appearance={theme}>`. Theme is persisted to `localStorage`.
**Why:** Radix UI's `Theme` component controls the appearance of all child components via CSS variables. Having a single context own the theme state and persist it ensures consistency across the app and across page reloads without flash of wrong theme.

## 2026-05-29 — feature/frontend-deploy

### Refresh token in localStorage instead of HttpOnly cookie
**Decision:** Refresh token stored in localStorage instead of HttpOnly cookie.
**Why:** Frontend (azurestaticapps.net) and backend (azurewebsites.net) are on different domains. Browsers block cross-site cookies (SameSite policy), so the HttpOnly cookie was never sent with refresh requests. localStorage works cross-domain.

### Email and JWT config via IConfiguration
**Decision:** Email and JWT secrets read via `_configuration["KEY"]` first (covers Azure App Settings), then fallback to nested config keys (covers local appsettings.Development.json).
**Why:** Azure App Settings are injected into IConfiguration directly as flat keys. Reading `_configuration["EMAIL_ADDRESS"]` works in Azure; `_configuration["Email:FromEmail"]` works locally. Order matters — Azure key must be checked first.

### appsettings.Development.json for local secrets
**Decision:** Local credentials stored in `appsettings.Development.json`, added to `.gitignore`.
**Why:** Keeps secrets out of source control while allowing local development without environment variables.

---

## 2026-05-31 — feature/clients

### Two separate services for Individual and Corporate clients
**Decision:** Split into `IndividualClientService` and `CorporateClientService` instead of one `ClientService`.
**Why:** Individual and Corporate clients have different creation logic, validation, and fields. Separate services keep each class focused and easier to test.

### ClientMapper static class
**Decision:** Static `ClientMapper` class in `Mappers/Clients/` instead of private mapping methods in each service.
**Why:** Mapping logic was duplicated across `IndividualClientService` and `CorporateClientService`. A shared mapper eliminates duplication and is reusable across controllers and services.

### RoleClaimType fix for ASP.NET Identity + JWT
**Decision:** Set `RoleClaimType` to the full Microsoft URI (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`) in both `TokenValidationParameters` and `Configure<IdentityOptions>`.
**Why:** `AddIdentity` overrides the JWT role claim type at runtime. Without this fix, `[Authorize(Roles)]` always returns 403 even when the JWT contains the correct role claim. Setting the same URI in both places ensures the JWT middleware and Identity pipeline agree on the claim key.

---

## 2026-05-31 — Clients tests

### Integration tests use Testing environment with InMemory DB
**Decision:** `Program.cs` checks `IsEnvironment("Testing")` and registers `UseInMemoryDatabase("TestDb")` instead of SQL Server. `WebApplicationFactory` sets the environment to `"Testing"` via `builder.UseEnvironment("Testing")`.
**Why:** Replacing DbContext service descriptors in `ConfigureServices` caused a dual-provider conflict at runtime — EF Core detected both the SQL Server and InMemory registrations and threw. Branching in `Program.cs` is cleaner: only one provider is ever registered, so no conflict is possible. Alternatives considered: removing all `DbContextOptions<>` descriptors (brittle, order-dependent) and using SQLite in-process (heavier dependency).

### EmailService mocked in integration tests
**Decision:** Real `EmailService` is replaced with a `Moq` mock in `WebApplicationFactory.ConfigureServices`.
**Why:** Integration tests call `CreateAsync` which invokes `SendWelcomeEmailAsync`. Without a mock, the test host tries to open a real SMTP connection to `localhost:25`, which fails in CI and dev environments. A Moq mock allows testing the full HTTP pipeline (auth → controller → service → repository) without side effects.

### xUnit 2.9.3 + xunit.runner.visualstudio 2.8.2
**Decision:** Pinned to xUnit 2.9.3 and `xunit.runner.visualstudio` 2.8.2.
**Why:** `xunit.runner.visualstudio` 3.x requires xUnit v3, which has breaking API changes and is not yet stable. Version 2.8.2 is the latest runner that is compatible with xUnit 2.x and works correctly with C# Dev Kit's test discovery in VS Code.

---

## 2026-05-31 — Employee ownership checks

### Employee data isolation for clients
**Decision:** Employees can only list, view, and update clients they personally created. Admins can access all clients regardless of who created them.
**Why:** Data isolation between employees — each employee manages their own client portfolio. Implemented via `createdByUserId` filter in `GetAllClientsAsync`, ownership check (`UnauthorizedException`) in `GetClientByIdAsync`, and `isAdmin` flag in `UpdateAsync` for both Individual and Corporate services.

### Dev OTP bypass for Employee role
**Decision:** In the Development environment, `OtpService` checks if the authenticating user has the `Employee` role and, if so, stores and returns a fixed OTP of `000000` without sending an email.
**Why:** Eliminates the need for multiple email accounts during local development and testing. Admin users still receive a real randomised OTP via SMTP so the full email flow is exercised in dev. Scoped to Development only — Azure (Production) always uses real OTPs.

---

## 2026-06-04 — feature/bank-accounts

### Nested routes for bank account open/list
**Decision:** Open and list bank accounts use client-scoped routes (`GET /api/clients/{clientId}/accounts`, `POST /api/clients/{clientId}/accounts`). Close uses an account-centric route (`PATCH /api/accounts/{id}/close`).
**Why:** Open and list are inherently client-scoped operations — the clientId is required input, so embedding it in the URL makes the ownership relationship explicit and RESTful. Close operates on a known account ID and does not need the clientId in the URL.

### IBAN uniqueness via DB index, not service-layer check alone
**Decision:** A unique index on `BankAccounts.IBAN` is added via migration (`AddBankAccountIbanUniqueIndex`). The service also calls `ExistsByIbanAsync` before insert and throws `ConflictException`.
**Why:** The service-layer check has a TOCTOU race condition under concurrent inserts. The DB index is the authoritative uniqueness guarantee. The service check provides a clean `409 Conflict` response before hitting the DB constraint, which would otherwise surface as an unhandled exception.

### BankAccountMapper static class
**Decision:** Static `BankAccountMapper` in `Mappers/BankAccounts/` following the same pattern as `ClientMapper`.
**Why:** Consistent with the established mapper pattern. Mapping logic is shared between the service and any future controllers without duplication.

---

## 2026-06-05 — feature/bank-accounts updates

### Soft delete on BankAccounts instead of hard delete
**Decision:** `BankAccount` has an `IsDeleted` bool (default false). `DeleteAsync` sets `IsDeleted = true`; `GetByIdAsync` and `GetAllByClientIdAsync` filter `IsDeleted = false`. Hard delete is never performed.
**Why:** Bank accounts have financial history (credits, installments, audit logs) that must not be permanently erased. Soft delete keeps the row for compliance while making the account invisible to all normal queries. The `DELETE /api/accounts/{id}` endpoint is the only path to soft-delete and is restricted to Admin.

### CloseAccount opened to Employee role
**Decision:** `PATCH /api/accounts/{id}/close` changed from `[Authorize(Roles = "Admin")]` to `[Authorize(Roles = "Employee,Admin")]`. Physical deletion (`DELETE /api/accounts/{id}`) remains Admin-only.
**Why:** Closing an account is a routine daily operation performed by employees during client off-boarding or account consolidation. Restricting it to Admin created an unnecessary bottleneck. Deletion is a destructive (irreversible even as soft-delete from the client perspective) action and stays Admin-only.

### OpenAccountAsync loads client with User via GetByIdWithDetailsAsync
**Decision:** `OpenAccountAsync` fetches the client using `GetByIdWithDetailsAsync` (which eager-loads the `User` navigation) instead of the base `GetByIdAsync`.
**Why:** The `IsActive` check requires `client.User.IsActive`. Using `GetByIdAsync` would return a `Client` with a null `User` navigation, requiring a second query or lazy loading. `GetByIdWithDetailsAsync` is already defined in `IClientRepository` and loads the full graph in one query.

### Inactive client guard on account opening
**Decision:** `OpenAccountAsync` throws `ValidationException("Cannot open an account for an inactive client.")` if `client.User.IsActive == false`.
**Why:** Opening an account for a deactivated client creates an orphaned financial record — the client cannot log in or be managed normally. Blocking at the service layer prevents this inconsistency and returns a clear `400 Bad Request` to the caller.

---

## 2026-06-14 — feature/account-transactions

### Ownership check on Close/Deposit/Withdraw via account.Client.CreatedByUserId
**Decision:** `CloseAccountAsync`, `DepositAsync`, and `WithdrawAsync` take `(id, requestingUserId, isAdmin)` and throw `UnauthorizedException("You do not have access to this account.")` if `!isAdmin && account.Client.CreatedByUserId != requestingUserId`. A new repository method `GetByIdWithClientAsync` eager-loads the `Client` navigation so the check requires no extra query.
**Why:** Employees must only operate on accounts belonging to clients they personally manage, consistent with the existing employee data-isolation model for `Clients` (see 2026-05-31 — Employee ownership checks). Ownership is anchored to the client record's creator (`Client.CreatedByUserId`), not the account's creator, so the rule stays consistent even if a different employee later opens an account for that client. Admins bypass the check entirely.

### Deposit/Withdraw blocked on non-Active accounts
**Decision:** `DepositAsync` throws `ValidationException("Cannot deposit to a closed account.")` and `WithdrawAsync` throws `ValidationException("Cannot withdraw from a closed account.")` if `account.Status != AccountStatus.Active`. `WithdrawAsync` additionally throws `ValidationException("Insufficient funds.")` if `account.Balance < amount`.
**Why:** A closed account should not accumulate further financial activity — allowing transactions on it would make the `Closed` status meaningless and complicate any future reconciliation. Insufficient-funds is a basic invariant for a debit operation.

### Zod `z.preprocess` + 3-generic `useForm` for numeric amount fields
**Decision:** `TransactionForm`'s `amount` field schema is `z.preprocess((val) => (val === '' || val == null ? undefined : Number(val)), z.number({ error: 'Amount is required' }).min(0.01, 'Amount must be greater than 0'))`. `useTransactionForm` calls `useForm<TransactionFormInput, unknown, TransactionFormOutput>` where `TransactionFormInput = z.input<typeof schema>` (`{ amount: unknown }`) and `TransactionFormOutput = z.output<typeof schema>` (`{ amount: number }`).
**Why:** A plain `z.number()` schema on a number input produces `NaN` (not `undefined`) when the field is cleared, which Zod reports as an unhelpful "Expected number, received nan" error. `z.preprocess` normalizes empty/invalid input to `undefined` so the `required_error`-equivalent message ("Amount is required") fires correctly. Because `preprocess` changes the input type to `unknown` while the output stays `number`, `zodResolver`'s inferred `Resolver` type no longer matches a single-generic `useForm<TransactionFormData>` — RHF's 3-generic `useForm<TFieldValues, TContext, TTransformedValues>` form resolves this without `any` casts.

---

## 2026-06-15 — feature/credit-services

### JsonStringEnumConverter for enum serialization
**Decision:** `JsonStringEnumConverter` added globally in `Program.cs` via `AddControllers().AddJsonOptions(...)`.
**Why:** `CreditType` enum is sent as string from frontend (`"Consumer"`/`"Mortgage"`). Global converter is cleaner than per-DTO `[JsonConverter]` attributes and is consistent with how `ClientType` and `AccountStatus` work as string values throughout the project.

### Radix Select requires key={field.value} for controlled value updates via reset()
**Decision:** Radix `Select` requires `key={field.value}` on `Select.Root` when value is controlled via React Hook Form `Controller` + `reset()`.
**Why:** Radix Select does not react to programmatic value changes after mount. Adding `key={field.value}` forces unmount/remount when the value changes, which correctly re-renders the selected option in edit mode.

---

## 2026-06-20 — feature/credits backend

### CreditServiceEntity alias in the CreditService service class
**Decision:** `CreditService` (the service class implementing `ICreditService`) uses `using CreditServiceEntity = BankOperations.Entities.CreditService;` to resolve the naming conflict with the service class itself.
**Why:** Both the entity (`Entities.CreditService`) and the service class (`Services.Credits.CreditService`) are named `CreditService`. The alias makes parameter and variable types readable (`CreditServiceEntity creditService`) without renaming either class — consistent with the same pattern already used in `Services/CreditServices/CreditServiceService.cs`.

### Credit update blocked once any installment is paid
**Decision:** `UpdateConsumerCreditAsync`/`UpdateMortgageCreditAsync` throw `ValidationException("Cannot update a credit with paid installments.")` if any `RepaymentInstallment.PaidAt != null` on the credit's current plan. Updates are also blocked if `Credit.Status != CreditStatus.Active`.
**Why:** Changing `Amount` or `TermMonths` requires regenerating the repayment plan from scratch. If installments have already been paid against the old plan's values, regenerating would silently invalidate that payment history (principal/interest breakdown, remaining balance) — there is no way to reconcile already-collected payments against a new schedule.

### Shared generic validation helper for credit updates
**Decision:** `ValidateAndPrepareUpdateAsync<T>(Guid id, Guid requestingUserId, bool isAdmin) where T : Credit` is a private helper in `CreditService` that loads the credit via `GetByIdWithDetailsAsync`, casts to `T` (throwing `NotFoundException` on mismatch), and runs the ownership check, active-status check, and paid-installments check. Both `UpdateConsumerCreditAsync` and `UpdateMortgageCreditAsync` call it as `await ValidateAndPrepareUpdateAsync<ConsumerCredit>(...)` / `<MortgageCredit>(...)`.
**Why:** Both update methods had identical validation logic (load → cast → ownership → status → paid-installments) before the credit-type-specific field assignment. Extracting it into a generic helper eliminates the duplication while the `where T : Credit` constraint keeps the cast and the returned type fully type-safe — no `as Credit` boxing or extra casting needed in the calling methods.

---

## 2026-06-20 — Parallel feature development

### Parallel feature development
**Decision:** `feature/employees`, `feature/activity-log`, and `feature/settings` are developed in parallel on separate feature branches and merged into `develop`.
**Why:** These features are independent from the remaining core work (credits frontend, installments, client portal, dashboards) and share no endpoints or components with the active development track. Parallel development reduces total delivery time without risk of merge conflicts.

---

## 2026-06-20 — feature/frontend-credits

### FormModal accepts an optional maxWidth prop
**Decision:** `FormModal` accepts an optional `maxWidth` prop (default `"480px"`).
**Why:** The RepaymentPlan table has many columns and needs more horizontal space. A generic prop keeps `FormModal` reusable without hardcoding widths per use case.

---

## 2026-06-20 — feature/employees backend

### Employee has no separate entity or table
**Decision:** Employee accounts are plain `ApplicationUser` rows with role `"Employee"` — no `Employee` entity, no TPT, no `Employees` table, no migration.
**Why:** Same reasoning as the original Client model before the `Clients` table existed: an Employee has no fields beyond what `AspNetUsers` already provides (`FirstName`, `LastName`, `Email`, `IsActive`, `CreatedAt`). Adding a table with zero extra columns would only add a join for no benefit. `IClientRepository`/`IClientService` proved this pattern works for accounts that are "just a user with a role" — `IEmployeeRepository` queries `ApplicationDbContext.Users` directly and filters by role via `UserManager.GetUsersInRoleAsync("Employee")` instead of a dedicated query.

### No ownership isolation for employees
**Decision:** `EmployeeService` has no `requestingUserId`/`isAdmin` ownership check anywhere — every method is reachable only by Admin (`[Authorize(Roles = "Admin")]` at the controller class level), and any Admin can view/deactivate/activate any employee.
**Why:** The Employee→Client ownership model (`Client.CreatedByUserId` filtering) exists because multiple Employees manage disjoint client portfolios. There is no equivalent concept for Employees themselves — Employees do not manage other Employees, and there is exactly one tier (Admin) above them. Adding an ownership check here would model a relationship that doesn't exist in the domain.

### Reuse PasswordGenerator and EmailService; ActivityLogService deferred
**Decision:** `EmployeeService` injects the existing `IPasswordGenerator` and `IEmailService` (`SendWelcomeEmailAsync`) from the Clients feature rather than writing employee-specific equivalents. `IActivityLogService.LogAsync` calls are intentionally **not** included in `CreateEmployeeAsync`/`DeactivateEmployeeAsync`/`ActivateEmployeeAsync`.
**Why:** Password generation and the welcome-email flow are identical regardless of role (Client vs Employee) — both create an `ApplicationUser` with a generated password and send the same email shape. Duplicating either would create two sources of truth for password rules and email templates. `IActivityLogService` is excluded because it does not exist in this branch yet — `feature/activity-log` (teammate's parallel branch) has not been merged into `develop`. The call sites are deliberately left out rather than stubbed against a temporary interface, to avoid a throwaway type that would need to be deleted and reconciled at merge time; logging will be added once the real `IActivityLogService` lands.

---

## 2026-06-20 — feature/employees frontend

### Create-only EmployeeForm — no edit mode
**Decision:** `EmployeeForm` only supports creation. There is no `UpdateEmployeeAsync` call, no edit `FormModal` instance, and no employee detail page — unlike `IndividualClientForm`/`CorporateClientForm`, which support both create and edit from day one.
**Why:** The backend has no `UpdateEmployeeAsync`/`PUT` endpoint for employees yet — it's listed only as a future extension in the employees backend knowledge doc, not implemented. Building an edit form against a non-existent endpoint would mean either a throwaway form or a half-wired one; matching frontend scope to actual backend capability keeps both in sync. Edit support can be added later by following the same pattern already proven on Clients.

---

## Template for new decisions

```markdown
### YYYY-MM-DD — Decision title
**Decision:** What was decided.
**Why:** Reasoning behind the decision. What alternatives were considered and why they were rejected.
```