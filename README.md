# 💰 BudgetManagement

A personal budget management web application built with **ASP.NET Core MVC** and **SQL Server**, designed to help users to track accounts, transactions, and financial categories in an organized way.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Database Setup](#database-setup)
- [Project Structure](#project-structure)
- [Usage](#usage)
- [API / Controllers](#api--controllers)
- [Roadmap](#roadmap)

---

## 📌 Overview

ManejoPresupuestos is a full-stack budget tracking application that allows users to manage their personal finances. Users can create account types (e.g. savings, credit cards, loans), manage accounts, log transactions, and organize spending by category. This would be the database system managing all relevant information related to budgets, users, reporting, accounts, among others.

---

## ✨ Features

- 🗂️ **Account Types** — Group accounts by category (e.g. loans, credit cards, cash)
- 🏦 **Accounts** — Create and manage individual financial accounts
- 💸 **Transactions** — Record income and expenses with date and description
- 🏷️ **Categories** — Classify transactions for better financial insight
- 🔃 **Drag & Drop Ordering** — Reorder account types interactively via jQuery UI Sortable
- 📊 **Dashboard** — Overview of your financial summary
- ✏️ **Full CRUD** — Create, read, update, and delete for all entities

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| Language | C# |
| ORM / Data Access | Dapper |
| Database | SQL Server |
| Frontend | Razor Views, Bootstrap 5 |
| JavaScript | jQuery, jQuery UI |
| HTTP Requests | Fetch API (AJAX) |
| IDE | Visual Studio / VS Code |

---

## 🗄️ Database Schema

Key tables used in the application:

```
[dbo].[account_types]      (account_type_id, name, user_id, order)
[dbo].[budget_account]     (account_id, name, balance, account_type_id, user_id, description)
[dbo].[category]           (category_id, name, operation_type_id, user_id)
[dbo].[transactions]       (transaction_id, account_id, category_id, amount, date, note, user_id)
[dbo].[operation_type]     (operation_type_id, description)
[dbo].[users]              (user_id, email, standard_email, password_hash)
```

> Relationships are enforced via Foreign Key constraints in SQL Server.

---

## 🌟 Getting Started

### Prerequisites

- [.NET SDK 7.0+](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express)
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) *(optional)*
- [Visual Studio 2022 or above](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   mkdir BudgetManagement
   git clone https://github.com/sebasflores10/BudgetManagement.git
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure the connection string** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=ManejoPresupuestos;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

### Database Setup

1. Open SSMS and connect to your SQL Server instance.
2. Run the SQL scripts located in `/Database/` directory in the following order:
   - `BudgetManagement_CreateTables.sql`

---

## 📁 Project Structure

```
ManejoPresupuestos/
├── Controllers/
│   ├── AccountTypeController.cs
│   ├── AccountController.cs
│   ├── TransactionController.cs
│   └── CategoryController.cs
├── Models/
│   ├── AccountType.cs
│   ├── Account.cs
│   ├── Transaction.cs
│   └── Category.cs
├── Services/
│   ├── IAccountTypeRepository.cs
│   └── AccountTypeRepository.cs
├── Views/
│   ├── AccountType/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/
│   ├── css/
│   └── js/
├── Database/
│   ├── 01_CreateTables.sql
│   └── 02_StoredProcedures.sql
├── appsettings.json
└── Program.cs
```

---

## 📖 Usage

Once the app is running, navigate to `https://localhost:7136` and:

1. **Create Account Types** — Define how you want to group your accounts.
2. **Add Budget Accounts** — Assign each account to an account type.
2. **Add Categories** — Assign each account to a category of an account type.
3. **Log Transactions** — Record your income and expenses.
4. **Explore the Dashboard** — View a summary of your financial activity.

> You can drag and drop rows in the Account Types table to reorder them. The order is saved automatically via an AJAX call to the backend.

---

## 🔌 API / Controllers

| Method | Route | Description |
|---|---|---|
| GET | `/AccountType` | List all account types |
| GET | `/AccountType/Create` | Create form |
| POST | `/AccountType/Create` | Save new account type |
| GET | `/AccountType/EditAccountType/{id}` | Edit form |
| POST | `/AccountType/EditAccountType/{id}` | Update account type |
| GET | `/AccountType/DeleteAccountType/{id}` | Delete confirmation |
| POST | `/AccountType/DeleteAccountType/{id}` | Delete account type |
| POST | `/AccountType/TableOrder` | Update sort order (AJAX) |

---

> Same pattern applicable to all other tables: [dbo].[account_types], [dbo].[budget_account], [dbo].[category], [dbo].[operation_type], [dbo].[transactions] & [dbo].[users]

## 🗺️ Roadmap

- [ ] User authentication & registration
- [ ] Monthly budget reports
- [ ] Chart visualizations (income vs. expenses)
- [ ] Export transactions to CSV
- [ ] Mobile-responsive improvements
- [ ] Dark mode

---