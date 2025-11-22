# Expense Tracker - AI Coding Instructions

## Project Overview
**Expense Tracker** is a personal finance management application built with Blazor Server, featuring transaction tracking, account management, category organization, and dashboard analytics with dark mode support.

**Tech Stack**: Blazor Server, EF Core, SQLite, ASP.NET Identity, MudBlazor, ApexCharts, xUnit

**Key Projects**: 
- `ExpenseTrackerWebApp` (main app) 
- `ExpenseTrackerTests` (xUnit tests with in-memory SQLite)

---

## Architecture Patterns

### Multi-Tenant Data Isolation
Every data entity (Account, Category, Transaction, UserPreferences) is scoped to `IdentityUserId`. Services enforce this via `ICurrentUserService.GetUserId()` to prevent cross-user data leaks.

**Pattern**: Query all user-specific data through helper methods like `GetAccountsQuery()` in `AccountService.cs` that filter by `IdentityUserId`.

### Service Layer Structure
Services (`Services/`) handle all business logic and data access:
- **Validation**: DTOs are validated before persistence (empty fields, null checks)
- **Data fetching**: Services use filtered queries to respect user boundaries
- **CRUD operations**: Save methods use `SetValues()` for updates, `AddAsync()` for inserts

**Example**: `AccountService.SaveAsync()` accepts `AccountDto`, validates fields, then calls `SaveInternal()` to persist.

### DTO Pattern
Data transfer uses DTOs (`Dtos/`) to separate API contracts from database models. Services convert between DTOs and models.

**Files**: `AccountDto`, `AccountWithBalance`, `TransactionDto`, `DashboardSummary`, etc.

### Razor Component Architecture
Dialog components (`Components/Pages/Dialogs/`) use MudBlazor dialogs for CRUD operations:
- Receive `[Parameter] EntityId` to determine add vs. edit mode
- Call `OnParametersSetAsync()` to load existing data
- Validate with `MudForm` before submission
- Close dialog with `MudDialog.Close(DialogResult.Ok())` on success

**Pattern**: See `AccountDialog.razor` for the standard flow.

---

## Critical Developer Workflows

### Building & Running
```powershell
# Restore & build solution
dotnet build ExpenseTracker.sln

# Run app (Blazor Server auto-compiles)
dotnet watch run --project ExpenseTrackerWebApp

# Run tests
dotnet test

# Docker deployment
docker-compose up
```

### Database Migrations
Migrations live in `Migrations/` and auto-run on app startup via `db.Database.Migrate()` in `Program.cs`. After model changes:
```powershell
dotnet ef migrations add MigrationName --project ExpenseTrackerWebApp
```

### Testing Approach
Tests use **in-memory SQLite** with disposable connections. Test setup:
1. Create connection in constructor
2. Seed test data (users, accounts, transactions)
3. Inject `TestCurrentUserService` (mock) to simulate logged-in users
4. Assert data isolation per user

**Key test pattern**: Multi-user scenarios verify that users can't access/modify each other's data.

---

## Project-Specific Conventions

### Naming & File Organization
- **Services**: `{Entity}Service.cs` handles all CRUD for that entity (e.g., `AccountService`, `TransactionService`)
- **Models**: Entity models in `Database/Models/` mirror database schema
- **Razor pages**: Top-level pages in `Components/Pages/`, dialogs in `Components/Pages/Dialogs/`
- **DTOs**: Mirror model names with "Dto" suffix (e.g., `AccountDto`)

### Validation Rules
- Services validate required fields before saving: non-empty strings, non-null amounts
- `AccountService.SaveAsync()` returns early (no-op) if validation fails—no exceptions thrown
- Database constraints: Accounts/Categories have cascade delete on user deletion, but Transaction→Category uses `Restrict` to prevent orphaned transactions

### Date & Time Handling
- All dates use `DateTime.Now` (local machine time)
- Dashboard calculations use `DateTime.StartOfMonth()` extension (from MudBlazor)
- Month grouping: `GroupBy(t => new { t.Date.Year, t.Date.Month })`
- Culture set to `en-US` in `Program.cs` for consistent formatting

### Decimal Precision
All money fields use `decimal` with `Precision(18, 2)` in `OnModelCreating()`:
```csharp
modelBuilder.Entity<Transaction>()
    .Property(t => t.Amount)
    .HasPrecision(18, 2);
```

### UI/UX Patterns (MudBlazor)
- **Cards**: Transaction/account cards use `MudCard` with `Outlined="true"` and `Class="pa-2 border-2"`
- **Dialogs**: Configured with `CloseOnEscapeKey=true`, `CloseButton=true`, `MaxWidth=Medium`
- **Snackbars**: Show success/error feedback; positioned bottom-left by default
- **Loading states**: Full-page spinners while fetching data (`MudProgressCircular Indeterminate="true"`)

### Delete Behavior
- Deleting an account cascades to delete all related transactions
- Deleting a category is restricted (must reassign transactions first)
- Confirm dialogs (`ConfirmDialog`) always prompt before destructive operations

---

## Cross-Component Communication

### Service Injection & Data Flow
Pages inject services, call them in `OnInitializedAsync()`, then pass data to child components via `[Parameter]`.

```
Page (Accounts.razor) 
  → AccountService.GetAllWithBalanceAsync() 
  → Display cards + Edit/Delete handlers 
  → Dialog (AccountDialog.razor) 
  → AccountService.SaveAsync() 
  → Snackbar.Add() + UpdatePage()
```

### State Management
- No global state; services are scoped
- Pages reload data after CRUD operations (`await UpdatePage()`)
- Dialogs return `DialogResult.Ok()` to signal success, parent refreshes

### Cascading Parameters
- `CascadingAuthenticationState` from `Program.cs` provides auth state to all components
- Dialog components receive `IMudDialogInstance` for closing
- Child components use `[CascadingParameter]` to access parent dialog instance

---

## External Dependencies & Integrations

- **MudBlazor 8.14.0**: UI components, dialogs, snackbars, forms
- **ApexCharts**: Line/pie charts on dashboard (configured in `Program.cs` with light theme)
- **CsvHelper 33.1.0**: Transaction CSV export (referenced in `ExportDialog`)
- **EF Core 9.0.0**: SQLite provider, design tools
- **ASP.NET Identity**: User authentication, roles (seeded demo user "sa" with admin role)
- **xUnit 2.9.2**: Test framework with `Fact` and `Theory` attributes

---

## Key Files Reference

| File | Purpose |
|------|---------|
| `Program.cs` | DI setup, middleware, database initialization, ApexCharts/MudBlazor config |
| `AppDbContext.cs` | EF Core models, relationships, migration metadata, seed data |
| `AccountService.cs` | Account CRUD with user isolation, balance calculations |
| `DashboardService.cs` | Monthly aggregations, category analytics |
| `Accounts.razor` | Account listing, add/edit/delete UI |
| `AccountDialog.razor` | Account CRUD dialog component (model for other dialogs) |
| `AccountServiceTests.cs` | Multi-user test scenarios with in-memory SQLite |
| `CurrentUserService.cs` | Claims-based user ID extraction for auth context |

---

## Common Pitfalls & Solutions

| Issue | Solution |
|-------|----------|
| Data visible across users | Always filter queries by `IdentityUserId` |
| Decimal precision loss | Use `Precision(18, 2)` in model builder |
| Stale UI after CRUD | Call `await UpdatePage()` in parent after dialog closes |
| Transaction cascade delete | Account deletion cascades; category deletion is restricted |
| Tests fail with schema mismatch | Seed test data matches migrations; use `EnsureCreated()` before seeding |

---

## Before Implementing Features

1. **User Isolation**: Does the new feature respect user boundaries?
2. **DTOs**: Do you need a DTO, or can services work directly with models?
3. **Dialog UI**: Are you adding CRUD? Use MudBlazor dialog pattern (see `AccountDialog.razor`)
4. **Validation**: What required fields must you check? Add to service `SaveAsync()` method
5. **Tests**: Multi-user scenarios? Write tests with different user IDs in `ExpenseTrackerTests/Tests/`
6. **Database**: New entities? Add to `AppDbContext`, create migration, seed test data

---

## Performance Notes (From Todo List)

- Dashboard & transaction queries need optimization for large datasets
- Mobile responsiveness incomplete—improve component layouts
- Loading states prevent UI hangs but may hide slow queries
- Test with 1000+ transactions to identify bottlenecks (sorting, filtering)
