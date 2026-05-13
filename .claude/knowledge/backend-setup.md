# Backend Setup

## Overview
ASP.NET Core Web API (.NET 10) project forming the foundation of the Bank Operations system. Establishes the folder structure, global error handling, and development tooling (Swagger).

## Location
- `backend/BankOperations/BankOperations.csproj` — project file, targets `net10.0`
- `backend/BankOperations/Program.cs` — app bootstrap, middleware pipeline
- `backend/BankOperations/Middleware/GlobalExceptionMiddleware.cs` — catches all exceptions, maps to HTTP responses
- `backend/BankOperations/Exceptions/NotFoundException.cs` — 404
- `backend/BankOperations/Exceptions/ConflictException.cs` — 409
- `backend/BankOperations/Exceptions/ValidationException.cs` — 400
- `backend/BankOperations/Controllers/HealthController.cs` — `GET /api/health`

## How it works

1. `Program.cs` builds the app with `AddControllers()`, `AddEndpointsApiExplorer()`, and `AddSwaggerGen()`
2. In Development, Swagger middleware is registered (`UseSwagger` + `UseSwaggerUI`)
3. `GlobalExceptionMiddleware` sits at the top of the pipeline — wraps every request in try/catch
4. When a service throws a custom exception, the middleware maps it to the correct HTTP status code and writes `{ "error": "..." }` JSON
5. `HealthController` provides a simple liveness check at `GET /api/health`

## Key details
- Framework is **net10.0** — do not downgrade to net8.0
- Swagger is wrapped in `if (app.Environment.IsDevelopment())` — never exposed in production (Azure)
- `GlobalExceptionMiddleware` uses a primary constructor (`RequestDelegate next`) — .NET 10 style
- `ValidationException` is aliased explicitly to avoid ambiguity with `System.ComponentModel.DataAnnotations.ValidationException`
- Unhandled exceptions fall through to 500 with the raw `exception.Message` — keep exception messages safe (no stack traces, no internal paths)
- `SaveChangesAsync()` is called in repositories, not in services — services orchestrate, repositories persist

## Code snippets

### Program.cs
```csharp
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
```

### GlobalExceptionMiddleware
```csharp
context.Response.StatusCode = exception switch
{
    NotFoundException => StatusCodes.Status404NotFound,
    ConflictException => StatusCodes.Status409Conflict,
    ValidationException => StatusCodes.Status400BadRequest,
    _ => StatusCodes.Status500InternalServerError
};

var body = JsonSerializer.Serialize(new { error = exception.Message });
await context.Response.WriteAsync(body);
```

### Throwing exceptions from services
```csharp
throw new NotFoundException("Client", id);       // → 404
throw new ConflictException("EGN already exists."); // → 409
throw new ValidationException("Amount must be positive."); // → 400
```

## Dependencies
- `Swashbuckle.AspNetCore` (v10.1.7) — Swagger UI for testing endpoints during development

## How to extend

### Add a new custom exception
1. Create `Exceptions/YourException.cs` extending `Exception`
2. Add a new case in the `switch` in `GlobalExceptionMiddleware.HandleExceptionAsync`

### Register a new service/repository
Add to `Program.cs` before `builder.Build()`:
```csharp
builder.Services.AddScoped<IYourRepository, YourRepository>();
builder.Services.AddScoped<IYourService, YourService>();
```

### Add Swagger JWT support (needed for feature/auth)
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
    c.AddSecurityRequirement(...);
});
```
