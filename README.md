# Messly

**Messly** is a production-oriented SaaS-style **Bachelor Mess Management System**. It helps flat managers track members, daily meals, shared expenses, member deposits, and monthly billing calculations (meal rate and balances).

Built with **.NET 10**, **Clean Architecture**, **Blazor Server**, **EF Core**, and **SQL Server**.

---

## Tech stack

| Layer | Technology |
|--------|------------|
| Runtime | .NET 10 |
| Web UI | Blazor Server (interactive) |
| API | ASP.NET Core Minimal API + JWT scaffold |
| Application | Services, DTOs, FluentValidation |
| Domain | Entities, enums, `BaseEntity` |
| Infrastructure | EF Core, SQL Server, Identity, repositories |
| Database | SQL Server (LocalDB for dev) |
| Logging | Serilog |
| Tests | xUnit + EF Core InMemory |

---

## Features

- **Members** — CRUD, roles (Manager/Member), soft delete, last-manager protection
- **Meals** — Daily grid entry (breakfast/lunch/dinner), monthly summary
- **Expenses** — CRUD with categories (auto-seeded), soft delete
- **Deposits** — CRUD per member, soft delete
- **Calculation / Reports** — Meal rate (`expenses ÷ meals`), member balances, monthly summary persistence
- **Dashboard** — Current-month snapshot
- **Settings** — Flat name, default meal rate, billing day
- **Auth** — ASP.NET Core Identity (Web), JWT scaffold (Api)

---

## Architecture

```
Messly.Domain          → No dependencies
Messly.Application     → Domain (interfaces, services, DTOs, validators)
Messly.Infrastructure  → Application + Domain (EF Core, repos, Identity)
Messly.Web             → Application + Infrastructure (Blazor UI)
Messly.Api             → Infrastructure (REST scaffold)
Messly.Tests           → Application + Infrastructure
```

**Dependency rule:** Domain ← Application ← Infrastructure ← Web / Api

- **Repository + Unit of Work** for persistence
- **Global soft delete** via EF query filters on `BaseEntity`
- **Flat scoping** via `FlatContextService` and `flat_id` claim (Web)

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) (or SQL Server instance)
- (Optional) [EF Core tools](https://learn.microsoft.com/ef/core/cli/dotnet) for migrations

---

## Setup (step-by-step)

### 1. Clone and restore

```bash
git clone <repository-url>
cd Messly
dotnet restore
```

### 2. Configure connection string

Edit `src/Messly.Web/appsettings.json` (and `src/Messly.Api/appsettings.json` if using Api):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MesslyDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 3. Apply database migrations

From the solution root:

```bash
dotnet ef database update \
  --project src/Messly.Infrastructure/Messly.Infrastructure.csproj \
  --startup-project src/Messly.Web/Messly.Web.csproj
```

> In **Development**, the Web app also runs `MigrateAsync()` and seeds data on startup.

### 4. Build and test

```bash
dotnet build
dotnet test
```

### 5. Run the Web app

```bash
dotnet run --project src/Messly.Web/Messly.Web.csproj
```

Open the URL shown in the console (typically `https://localhost:7xxx`).

---

## Default login (development)

| Field | Value |
|--------|--------|
| Email | `manager@messly.local` |
| Password | `Manager@123` |

Seeded flat ID (config): `00000000-0000-0000-0000-000000000001`

---

## Run the API (optional)

```bash
dotnet run --project src/Messly.Api/Messly.Api.csproj
```

Swagger is enabled in Development at `/swagger`.

> **Note:** API endpoints require JWT authentication, but a token-issuing login endpoint is not implemented yet. Use the Web app for full functionality.

---

## Folder structure

```
Messly/
├── src/
│   ├── Messly.Domain/           # Entities, enums, BaseEntity
│   ├── Messly.Application/      # Services, DTOs, validators, interfaces
│   ├── Messly.Infrastructure/   # DbContext, migrations, repositories, Identity
│   ├── Messly.Web/              # Blazor Server UI
│   └── Messly.Api/              # Minimal API
├── tests/
│   └── Messly.Tests/            # Unit tests (services)
├── .github/workflows/ci.yml     # Build + test on push/PR
└── Messly.slnx
```

---

## Testing each module

After logging in, use the sidebar or routes below.

### Member module

| Step | Route / action |
|------|----------------|
| List | `/members` |
| Add | `/members/add` |
| Edit | `/members/edit/{id}` |
| Delete | List → Delete (confirm) |

**Checks:** Unique email, role selection, cannot delete last manager.

### Expense module

| Step | Route / action |
|------|----------------|
| List | `/expenses` |
| Add / Edit | `/expenses/add`, `/expenses/edit/{id}` |
| Categories | Auto-created on first use (Grocery, Utility, etc.) |

**Checks:** Amount > 0, category and payer required.

### Deposit module

| Step | Route / action |
|------|----------------|
| List | `/deposits` |
| Add / Edit | `/deposits/add`, `/deposits/edit/{id}` |

**Checks:** Amount > 0, member and date required.

### Meal module

| Step | Route / action |
|------|----------------|
| Daily entry | `/meals/entry` |
| Monthly summary | `/meals/summary` |

**Checks:** Counts 0–3 per meal type; save and reload same date.

### Calculation module

| Step | Route / action |
|------|----------------|
| Monthly summary | `/reports/monthly` |
| Member balances | `/reports/balances` |
| Persist snapshot | Monthly Summary → **Recalculate & Save** |

**Checks:** Meal rate = total expenses ÷ total meals; balance = deposits − (meals × rate).

### Automated tests

```bash
dotnet test --filter "FullyQualifiedName~MemberService"
dotnet test --filter "FullyQualifiedName~ExpenseService"
dotnet test --filter "FullyQualifiedName~DepositService"
dotnet test --filter "FullyQualifiedName~MealService"
dotnet test --filter "FullyQualifiedName~BillingCalculation"
```

---

## Database migrations

**Create a new migration** (after model changes):

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Messly.Infrastructure/Messly.Infrastructure.csproj \
  --startup-project src/Messly.Web/Messly.Web.csproj \
  --output-dir Data/Migrations
```

**Apply migrations:**

```bash
dotnet ef database update \
  --project src/Messly.Infrastructure/Messly.Infrastructure.csproj \
  --startup-project src/Messly.Web/Messly.Web.csproj
```

Existing migrations:

- `20260518152358_InitialCreate`
- `20260518155013_AddIdentityTables`

---

## Troubleshooting

### Build fails

- Ensure **.NET 10 SDK** is installed: `dotnet --version`
- Run `dotnet restore` then `dotnet build`

### Database / migration errors

- Confirm LocalDB is installed and the connection string is correct
- Run `dotnet ef database update` manually
- Delete the database and re-run update if schema is out of sync (dev only)

### Cannot log in

- Use dev credentials: `manager@messly.local` / `Manager@123`
- Run the app in **Development** so `DevDataSeeder` and `IdentityDataSeeder` run
- Check `Messly:DefaultFlatId` in `appsettings.json`

### Empty lists after login

- Verify `flat_id` claim and `Messly:DefaultFlatId` match the seeded flat
- Add members before meals/deposits

### API returns 401

- JWT login endpoint is not implemented; authenticate via Web or add token issuance

### Blazor circuit disconnected

- Refresh the page; check server logs (Serilog console output)

---

## Health check

```
GET /health
```

Returns database health when configured.

---

## CI

GitHub Actions (`.github/workflows/ci.yml`) runs on `main`, `master`, and `develop`:

- `dotnet restore`
- `dotnet build -c Release`
- `dotnet test -c Release`

---

## License

Proprietary — Code Synapse Technology LTD (adjust as needed).
