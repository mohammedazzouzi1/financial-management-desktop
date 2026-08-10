# Mizan Finance

**Your finances. One clear view.**
*Toute votre gestion financière, au même endroit.*

A professional, offline-first Windows desktop financial management application for businesses: cash and bank accounts, transactions, cheques, invoices, clients/suppliers, receivables/payables, reports and analytics — all in one place.

## Status

Early development. The current build covers the architecture foundation plus a first vertical slice: authentication, first-run setup, dashboard, transactions, cash register (caisse), and bank accounts. See [Releases](../../releases) for tagged milestones.

## Tech stack

- **.NET 8** / **WPF** (`net8.0-windows`)
- **SQLite** via **EF Core** (offline-first local database, code-first migrations)
- **CommunityToolkit.Mvvm** for MVVM (observable view models, relay commands)
- **WPF-UI** (Fluent Design controls) + a custom fintech theme for the UI
- **LiveCharts2** (`LiveChartsCore.SkiaSharpView.WPF`) for dashboard charts
- **BCrypt.Net** for password hashing

## Solution structure

```
MizanFinance.sln
src/
  MizanFinance.Core/    Domain entities, enums, DTOs, service interfaces
  MizanFinance.Data/    EF Core DbContext, migrations, service implementations, seeders
  MizanFinance.App/     WPF UI: views, view models, DI wiring, theming
```

## Getting started

Requires the .NET 8 SDK (or later, with the `net8.0-windows` desktop runtime).

```powershell
dotnet build MizanFinance.slnx
dotnet run --project src/MizanFinance.App
```

On first launch, the app walks through a setup wizard (company info, default currency, administrator account, first cash account, optional bank account, and optional demo data) before opening the dashboard. Application data lives in `%LocalAppData%\MizanFinance\`.

## What's implemented so far

- **Data layer**: accounts (cash/bank), categories, clients, suppliers, transactions, daily cash register, audit log, company settings — with automatic balance updates on every transaction (revenue/expense/transfer), and cheque payments deferred from cash balance until a future cheque-clearing module lands.
- **Auth**: login, BCrypt password hashing, role field (Administrator/Manager/Accountant/Viewer).
- **First-run wizard**: company info → currency → admin account → cash account → bank account → categories overview → optional demo data.
- **Dashboard**: KPI cards (cash/bank/total balance, today's revenue/expenses, monthly profit) and revenue-vs-expense / cash-flow charts with date-range presets.
- **Transactions**: searchable/filterable grid, add/edit/delete with account-balance-aware create/update/delete logic.
- **Cash register (Caisse)**: daily opening/closing balance, automatic discrepancy calculation, 14-day history.
- **Bank accounts**: CRUD, balance-evolution chart, quick transaction entry.
- **Settings**: company info editing, demo-data removal.

Not yet implemented: cheques, invoices, receivables/payables, expense/revenue reporting, analytics, documents, backup/restore, localization beyond French UI copy, and Windows installer packaging — see the project roadmap for the full phased plan.
