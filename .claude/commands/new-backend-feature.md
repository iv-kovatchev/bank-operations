# New Backend Feature Command

Follow these steps exactly when creating a new backend feature. Read @CONVENTIONS.md before starting.

## Steps

### 1. Entities (if needed)
- Create entity class in `Entities/[Feature]/`
- Extend `BaseEntity` for `Guid Id`
- Add navigation properties
- Register in `ApplicationDbContext`
- Add TPT configuration in `OnModelCreating` if inheritance is used

### 2. DTOs
- Create in `DTOs/[Feature]/`
- `Create[Feature]Dto` — input for creation
- `Update[Feature]Dto` — input for update (if needed)
- `[Feature]ResponseDto` — always returned from controller, never the entity

### 3. Repository
- Create `IGenericRepository<T>` — already exists, extend it
- Create `I[Feature]Repository` in `Repositories/[Feature]/` extending `IGenericRepository<[Entity]>`
- Create `[Feature]Repository` implementing `I[Feature]Repository`
- Write full implementation — no base class
- Register in `Program.cs`: `builder.Services.AddScoped<I[Feature]Repository, [Feature]Repository>()`

### 4. Service
- Create `I[Feature]Service` in `Services/[Feature]/` extending `IGenericService<[Entity]>`
- Create `[Feature]Service` implementing `I[Feature]Service`
- Inject repository via constructor
- Throw custom exceptions from `Exceptions/` — never raw exceptions
- Call `_activityLogService.LogAsync(...)` after every operation
- Register in `Program.cs`: `builder.Services.AddScoped<I[Feature]Service, [Feature]Service>()`

### 5. Controller
- Create `[Feature]Controller` in `Controllers/`
- Inject service via constructor
- Keep controllers thin — only HTTP handling, delegate to service
- Use `[Authorize]` — specify role if needed: `[Authorize(Roles = "Admin")]`
- Get current user: `Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`
- Always return DTOs, never entities

### 6. Migration
```bash
cd backend/BankOperations
dotnet ef migrations add Add[Feature]
dotnet ef database update
```

### 7. After completion
- Update `PROGRESS.md` — mark feature as done
- Add decision to `DECISIONS.md` if any architectural choice was made
- Suggest next step