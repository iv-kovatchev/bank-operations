# Bank Operations System — PROJECT.md

> This file is the "brain" of the project. When starting a new chat with AI — share this file alongside PROGRESS.md and CONVENTIONS.md for full context.

---

## 📋 Description

A web application for managing clients, bank accounts, and credit services at a bank. Employees manage client data and financial operations; clients can log in to view their own accounts, credits, and repayment plans (read-only).

---

## 🛠 Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | C# + ASP.NET Core Web API (.NET 10) |
| Frontend | React + TypeScript |
| Database | Azure SQL (SQL Server) |
| ORM | Entity Framework Core 10 (Code First) |
| Auth | ASP.NET Identity + JWT (Access Token 15min + Refresh Token in HttpOnly cookie) |
| Email | SendGrid |
| CI/CD | GitHub Actions → Azure App Service |
| Version Control | GitHub |

---

## 🌿 Git Branching Strategy

- `develop` — main branch
- `feature/xxx` — feature branches, merged into `develop`
- Deploy: push to `develop` → automatically deploys to Azure App Service via GitHub Actions

---

## 👥 Roles

Three roles exist in `AspNetUsers`. Each role determines what actions the user can perform.

### Admin
- Creates accounts for Admin, Employee, and Client users
- On creation: password is generated → template email sent via SendGrid
- Deactivates employees ("firing") — `IsActive = false`
- Views Activity Log of all employees

### Employee
- Creates clients (individual & corporate) — this automatically creates an AspNetUsers account with role Client
- Opens bank accounts
- Grants credits (consumer & mortgage)
- Generates repayment plans (annuity)
- Marks installments as paid
- Checks credit status

### Client
- Read-only access to their own data: bank accounts, credits, repayment plans
- Cannot modify any data
- Account is created automatically during client registration (by Employee or Admin)

---

## 📁 Project Structure

```
bank-operations/
├── backend/
│   └── BankOperations/
│       ├── Controllers/
│       ├── Data/
│       │   └── ApplicationDbContext.cs
│       ├── Entities/
│       │   ├── Clients/
│       │   │   ├── Client.cs
│       │   │   ├── IndividualClient.cs
│       │   │   └── CorporateClient.cs
│       │   ├── Credits/
│       │   │   ├── Credit.cs
│       │   │   ├── ConsumerCredit.cs
│       │   │   └── MortgageCredit.cs
│       │   ├── BankAccount.cs
│       │   ├── CreditService.cs
│       │   ├── RepaymentPlan.cs
│       │   ├── RepaymentInstallment.cs
│       │   └── ActivityLog.cs
│       ├── Enums/
│       ├── DTOs/
│       ├── Exceptions/
│       │   ├── NotFoundException.cs
│       │   ├── ValidationException.cs
│       │   └── ConflictException.cs
│       ├── Repositories/
│       │   ├── Base/
│       │   │   └── IGenericRepository.cs
│       │   ├── Clients/
│       │   │   ├── IClientRepository.cs
│       │   │   └── ClientRepository.cs
│       │   └── ...
│       └── Services/
│           ├── Base/
│           │   └── IGenericService.cs
│           ├── Clients/
│           │   ├── IClientService.cs
│           │   └── ClientService.cs
│           └── ...
├── frontend/
│   └── src/
│       ├── components/
│       ├── pages/
│       ├── services/
│       ├── store/
│       ├── types/
│       ├── hooks/
│       └── utils/
├── .cursor/
│   └── rules
├── PROJECT.md
├── PROGRESS.md
├── CONVENTIONS.md
├── README.md
└── .gitignore
```

---

## 🗄 Database Architecture

### Inheritance Strategy: TPT (Table Per Type)
EF Core configuration:
```csharp
modelBuilder.Entity<IndividualClient>().ToTable("IndividualClients");
modelBuilder.Entity<CorporateClient>().ToTable("CorporateClients");
modelBuilder.Entity<ConsumerCredit>().ToTable("ConsumerCredits");
modelBuilder.Entity<MortgageCredit>().ToTable("MortgageCredits");
```

### Tables

#### AspNetUsers (extended IdentityUser)
```
Id, Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt
```

#### Clients (base class)
```
ClientId (PK, FK → AspNetUsers), CreatedByUserId (FK → AspNetUsers)
```
> ClientId = AspNetUsers.Id — the client IS an AspNetUsers account (1:1).
> IsActive and CreatedAt are read from AspNetUsers — not duplicated here.
> CreatedByUserId = the Employee or Admin who registered this client.

#### IndividualClients (1:1 with Clients — TPT)
```
ClientId (PK, FK), FirstName, LastName, EGN
```

#### CorporateClients (1:1 with Clients — TPT)
```
ClientId (PK, FK), CompanyName, EIK, RepresentativeFirstName, RepresentativeLastName
```

#### BankAccounts
```
Id, IBAN, Balance, Status, ClientId (FK), CreatedAt, CreatedByUserId (FK)
```

#### CreditServices (credit product configuration)
```
Id, Type (Consumer/Mortgage), InterestRate, MaxAmount, MaxTermMonths
```

#### Credits (base class)
```
Id, ClientId (FK), CreditServiceId (FK), Amount, TermMonths, Status, CreatedAt, CreatedByUserId (FK)
```

#### ConsumerCredits (1:1 with Credits — TPT)
```
CreditId (PK, FK), Purpose (enum: CarPurchase/HomeRenovation/Education/Other)
```

#### MortgageCredits (1:1 with Credits — TPT)
```
CreditId (PK, FK), PropertyAddress, PropertyType (enum: Apartment/House/Commercial)
```

#### RepaymentPlans (1:1 with Credits)
```
Id, CreditId (FK, unique), MonthlyInstallment, GeneratedAt
```

#### RepaymentInstallments (1:N with RepaymentPlans)
```
Id, RepaymentPlanId (FK), InstallmentNumber, DueDate, PrincipalPart, InterestPart, RemainingBalance, PaidAt (nullable), CreatedByUserId (FK, nullable)
```
> TotalAmount is omitted — always derived as `PrincipalPart + InterestPart`.
> IsPaid is omitted — determined by `PaidAt != null`.
> CreatedByUserId = the employee who marked the installment as paid.

#### ActivityLogs
```
Id, UserId (FK), Action, EntityType, EntityId, Timestamp, Details
```

---

## ⚙️ Functional Requirements

- [ ] Add client (individual / corporate)
- [ ] Open bank account
- [ ] Grant credit (consumer / mortgage)
- [ ] Generate repayment plan (annuity)
- [ ] Mark installment as paid
- [ ] Check credit status
- [ ] Employee management (Admin)
- [ ] Activity Log (Admin)

---

## 💡 Key Architectural Decisions

### Why single project structure?
Simpler to navigate, easier to maintain for a solo developer. No over-engineering — all folders are in one ASP.NET project.

### Why Generic interfaces only (no generic implementations)?
Each repository and service has specific logic. Generic interfaces enforce a consistent contract while allowing full flexibility in each implementation.

### Why custom Exceptions?
Clean error handling — services throw typed exceptions (NotFoundException, ConflictException), global middleware catches them and returns structured JSON responses.

### Why TPT (Table Per Type)?
The assignment requires inheritance for clients and credits. TPT creates a separate table for each subclass with a 1:1 relation to the base table — making inheritance clearly visible in the DB schema.

### Why JWT + Refresh Token?
REST API + React SPA architecture. Short-lived access token (15 min) + Refresh token in HttpOnly cookie for security.

### Why CreditServices table?
Interest rate, maximum amount, and term are configured per credit type. Stored in DB so they can be changed without a redeploy.

### Why ActivityLogs?
Admin functionality — tracking all employee actions. Automatically recorded on every operation.

### Why three roles (Admin, Employee, Client)?
Clients need read-only access to their own data via the same API. A third role keeps authorization clean — `[Authorize(Roles = "Client")]` on read endpoints, `[Authorize(Roles = "Employee,Admin")]` on write endpoints.

### Why is Clients.ClientId a FK to AspNetUsers?
Each client is also a user — they need to log in. The 1:1 relationship eliminates a redundant Id column and ensures there is no orphaned client record without a user account.

### Why is client registration a single step (no separate user creation)?
Separate steps would allow an employee to create a client record without a login account, leaving the system in an inconsistent state. Combining them in one service transaction guarantees both records are always created together or neither is.

### Why are TotalAmount and IsPaid removed from RepaymentInstallments?
Both are derivable from existing columns: `TotalAmount = PrincipalPart + InterestPart`, `IsPaid = PaidAt != null`. Storing derived values risks inconsistency if either source value changes. Computed properties in the entity are sufficient.

### Repayment Plan — Annuity Formula
```
M = P * [r(1+r)^n] / [(1+r)^n - 1]
where: P = principal, r = monthly interest rate, n = number of months
```

---

## 🚀 CI/CD Pipeline

- Trigger: push to `develop`
- Steps: Build → Test → Deploy to Azure App Service