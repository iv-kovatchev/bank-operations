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

### CreditServices table
**Decision:** Interest rate, max amount, and max term are stored in a `CreditServices` table, not hardcoded.
**Why:** The assignment states these values are determined by credit type. Storing them in DB allows changes without redeployment.

### Custom Exceptions
**Decision:** Custom exception classes (NotFoundException, ConflictException, ValidationException) caught by global middleware.
**Why:** Clean separation — services throw typed exceptions, middleware handles HTTP response mapping. Controllers stay thin and don't need try/catch blocks.

### ActivityLogs table
**Decision:** Every employee action is recorded in an `ActivityLogs` table.
**Why:** Admin requirement — admins must be able to see who did what and when. Implemented as a service called from every operation.

### Chakra UI for frontend
**Decision:** Chakra UI instead of Tailwind CSS or MUI.
**Why:** Provides ready-made accessible components suitable for an admin-style banking application. Faster development than Tailwind (no need to compose utilities), more flexible than MUI.

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

## Template for new decisions

```markdown
### YYYY-MM-DD — Decision title
**Decision:** What was decided.
**Why:** Reasoning behind the decision. What alternatives were considered and why they were rejected.
```