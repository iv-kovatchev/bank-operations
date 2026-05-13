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

## Template for new decisions

```markdown
### YYYY-MM-DD — Decision title
**Decision:** What was decided.
**Why:** Reasoning behind the decision. What alternatives were considered and why they were rejected.
```