# Bank Operations System — [PROGRESS.md](http://PROGRESS.md)

> Update this file after every merge into `develop`. Cursor updates it automatically after each completed task.

---

## ✅ Completed

### Architecture & Planning

- Finalized database schema (TPT inheritance for Clients and Credits)
- Defined roles: Admin, Employee, Client
- Defined all functional requirements
- Decided on single-project backend structure (no layered solution)
- Decided on generic interfaces only — no generic implementations
- Created ER diagram (draw.io) and DB schema (dbdiagram.io)

### Documentation

- `PROJECT.md` created
- `PROGRESS.md` created
- `CONVENTIONS.md` created
- `README.md` created
- `.gitignore` created
- `.cursor/rules` created

### Phase 1 — Foundation

- [x] `feature/backend-setup` — ASP.NET Core Web API (.NET 10) project structure + `/health` endpoint — `2026-05-13`
  - Folder structure: Controllers, Data, DTOs, Entities, Enums, Exceptions, Middleware, Repositories, Services
  - `GlobalExceptionMiddleware` — maps NotFoundException→404, ConflictException→409, ValidationException→400
  - Custom exceptions: `NotFoundException`, `ConflictException`, `ValidationException`
  - `GET /api/health` → `{ "status": "healthy" }`
  - Swagger UI added (Swashbuckle.AspNetCore) — available at `/swagger` in Development only

- [x] `feature/ci-cd` — GitHub Actions pipeline → Azure App Service deploy — `2026-05-14`
  - Workflow file: `.github/workflows/develop_bank-operations-api.yml`
  - Trigger: push to `develop` branch with path filter `backend/**` (+ manual `workflow_dispatch`)
  - Build job: checkout → setup .NET 10 → `dotnet build --configuration Release` → `dotnet publish` → upload artifact
  - Deploy job: download artifact → Azure OIDC login (Federated Identity, no client secret) → `azure/webapps-deploy@v3` to `bank-operations-api` Production slot
  - Azure credentials stored as GitHub secrets (`AZUREAPPSERVICE_CLIENTID_*`, `AZUREAPPSERVICE_TENANTID_*`, `AZUREAPPSERVICE_SUBSCRIPTIONID_*`)

---

## 🔄 In Progress

- Nothing in progress yet

---

## 📋 Backlog (in order)

### Phase 1 — Foundation

- [x] `feature/backend-setup` — ASP.NET Core Web API (.NET 10) project structure + `/health` endpoint
- [x] `feature/ci-cd` — GitHub Actions pipeline → Azure App Service deploy
- [ ] `feature/database-models` — EF Core entities, DbContext, migrations, Azure SQL
- [ ] `feature/auth` — ASP.NET Identity + JWT + Refresh Token + SendGrid email (employee creation by Admin; client creation by Employee/Admin)

### Phase 2 — Frontend Foundation

- [ ] `feature/frontend-setup` — React + TypeScript + Chakra UI + Axios + routing
- [ ] `feature/frontend-auth` — Login page + JWT interceptors + protected routes

### Phase 3 — Core Features (backend + frontend in parallel)

- [ ] `feature/clients` + `feature/frontend-clients` — Clients CRUD (Individual + Corporate)
- [ ] `feature/bank-accounts` + `feature/frontend-accounts` — Bank Accounts CRUD
- [ ] `feature/credits` + `feature/frontend-credits` — Credits (Consumer + Mortgage) + Repayment Plan generation
- [ ] `feature/installments` — Mark installment as paid + credit status check
- [ ] `feature/activity-log` + `feature/frontend-admin` — Activity Log middleware + Employee management + Admin view

---

## 🐛 Known Issues

- None yet

---

## 📌 Notes & Decisions Made During Development

- `2026-05-13` — Used .NET 10 (not .NET 8) — .NET 10 is installed on the dev machine
- `2026-05-13` — Swagger gated behind `IsDevelopment()` — will not be exposed on Azure
- `2026-05-14` — CI/CD uses Azure OIDC Federated Identity (no client secret stored) — credentials managed via GitHub secrets with scoped permissions per job
- `2026-05-15` — Revised roles: three roles (Admin, Employee, Client); Client role has read-only self-service access
- `2026-05-15` — Client registration is a single-step transaction: creates AspNetUsers account (role=Client) + Client/IndividualClient or CorporateClient record + sends welcome email
- `2026-05-15` — Clients table: removed Status (use IsActive from AspNetUsers) and CreatedAt (use CreatedAt from AspNetUsers); ClientId is now PK and FK → AspNetUsers (1:1)
- `2026-05-15` — RepaymentInstallments: removed TotalAmount (= PrincipalPart + InterestPart, derived) and IsPaid (= PaidAt != null, derived)

