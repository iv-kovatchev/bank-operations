# Bank Operations System — [PROGRESS.md](http://PROGRESS.md)

> Update this file after every merge into `develop`. Cursor updates it automatically after each completed task.

---

## ✅ Completed

### Architecture & Planning

- Finalized database schema (TPT inheritance for Clients and Credits)
- Defined roles: Admin, Employee
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

---

## 🔄 In Progress

- Nothing in progress yet

---

## 📋 Backlog (in order)

### Phase 1 — Foundation

- [ ] `feature/ci-cd` — GitHub Actions pipeline → Azure App Service deploy
- [ ] `feature/database-models` — EF Core entities, DbContext, migrations, Azure SQL

### Phase 2 — Auth

- [ ] `feature/auth` — ASP.NET Identity + JWT + Refresh Token + SendGrid email on employee creation

### Phase 3 — Core Features

- [ ] `feature/clients` — Clients CRUD (Individual + Corporate)
- [ ] `feature/bank-accounts` — Bank Accounts CRUD
- [ ] `feature/credits` — Credits (Consumer + Mortgage) + Repayment Plan generation
- [ ] `feature/installments` — Mark installment as paid + credit status check
- [ ] `feature/activity-log` — Activity Log middleware + Admin view

### Phase 4 — Frontend

- [ ] `feature/frontend-setup` — React + TypeScript + Chakra UI + Axios + routing
- [ ] `feature/frontend-auth` — Login page + JWT interceptors + protected routes
- [ ] `feature/frontend-clients` — Clients pages
- [ ] `feature/frontend-accounts` — Bank accounts pages
- [ ] `feature/frontend-credits` — Credits + repayment plan pages
- [ ] `feature/frontend-admin` — Employee management + Activity Log pages

---

## 🐛 Known Issues

- None yet

---

## 📌 Notes & Decisions Made During Development

- `2026-05-13` — Used .NET 10 (not .NET 8) — .NET 10 is installed on the dev machine
- `2026-05-13` — Swagger gated behind `IsDevelopment()` — will not be exposed on Azure

