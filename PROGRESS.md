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

---

## 🔄 In Progress

- Nothing in progress yet

---

## 📋 Backlog (in order)

### Phase 1 — Foundation

- [ ] `feature/backend-setup` — ASP.NET Core project structure + `/health` endpoint
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

- [ ] `feature/frontend-setup` — React + TypeScript + Tailwind + Axios + routing
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

> Add notes here as development progresses. Example:
>
> - `2024-01-15` — Decided to use Zustand instead of Redux Toolkit — simpler for this project size

