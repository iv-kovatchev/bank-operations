# Create Migration Command

Run this after adding or modifying entities.

## Steps

### 1. Verify DbContext
- Make sure the new entity is registered in `ApplicationDbContext`
- Make sure any TPT configuration is added in `OnModelCreating`

### 2. Create migration
```bash
cd backend/BankOperations
dotnet ef migrations add [MigrationName]
```

Migration naming convention:
- `InitialCreate` — first migration
- `AddClients` — adding clients tables
- `AddCredits` — adding credits tables
- `AddRepaymentPlan` — adding repayment plan tables

### 3. Review the migration
- Always open and read the generated migration file
- Make sure it matches what you expect
- Check that no unintended changes are included

### 4. Apply migration
```bash
dotnet ef database update
```

### 5. If something is wrong
```bash
# Remove last migration (only if not applied yet)
dotnet ef migrations remove
```