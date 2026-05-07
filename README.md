# 🏦 Bank Operations

## 📖 Overview

Bank Operations is an internal banking management system developed as a university practice project.

The application is intended for internal use by bank employees and administrators. The system allows management of bank clients, bank accounts, loans and installment payments through a centralized web platform.

---

# ⚙️ Main Functionalities

- 👤 Management of individual and company clients
- 💳 Opening and managing bank accounts
- 💰 Managing consumer and mortgage loans
- 📊 Generating annuity repayment plans
- ✅ Marking installments as paid
- 📌 Tracking loan status

---

# 🛠️ Technologies

## 🔹 Backend
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication

## 🎨 Frontend
- React
- TypeScript
- MUI
- React Query

## ☁️ DevOps
- GitHub
- GitHub Actions
- Azure App Service

---

# 🏗️ Architecture

The project follows a layered architecture approach:

- Controllers
- Services
- Repositories
- Database

Controllers do not access repositories directly.

---

# 👥 User Roles

## 🛡️ Admin
- Manages employees
- Full system access

## 👨‍💼 Employee
- Manages clients
- Opens bank accounts
- Creates loans
- Manages installment payments

---

# 🧩 Domain Model

## 👤 Client Inheritance

Client (abstract)
- IndividualClient
- CompanyClient

## 💰 Loan Inheritance

Loan (abstract)
- ConsumerLoan
- MortgageLoan

---

# 🗄️ Database

- SQL Server relational database
- TPT inheritance strategy for Clients and Loans

---

# 📁 Project Structure

```txt
backend/
frontend/
docs/