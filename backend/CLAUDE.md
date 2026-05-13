# CLAUDE.md — Backend

## Context
Read @CONVENTIONS.md for exact code patterns used in this project.

## Project Structure
```
BankOperations/
├── Controllers/
├── Data/
│   └── ApplicationDbContext.cs
├── Entities/
│   ├── Clients/
│   ├── Credits/
│   └── ...
├── Enums/
├── DTOs/
├── Exceptions/
├── Repositories/
│   ├── Base/
│   │   └── IGenericRepository.cs
│   └── [Feature]/
│       ├── I[Feature]Repository.cs
│       └── [Feature]Repository.cs
└── Services/
    ├── Base/
    │   └── IGenericService.cs
    └── [Feature]/
        ├── I[Feature]Service.cs
        └── [Feature]Service.cs
```

## Key Rules
- Repository Pattern + Service Layer — no business logic in controllers
- Generic interfaces only — no generic implementations
- Always use DTOs — never return entities from controllers
- Custom exceptions from `Exceptions/` folder — never throw raw exceptions
- TPT inheritance for Clients and Credits
- `decimal(18,2)` for all monetary values
- `Guid` for all primary keys
- `async/await` everywhere

## Registrations
All repositories and services must be registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();
```

## Error Responses
Global middleware handles all exceptions:
- `NotFoundException` → 404 `{ "error": "..." }`
- `ConflictException` → 409 `{ "error": "..." }`
- `ValidationException` → 400 `{ "error": "..." }`