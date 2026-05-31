# Clients Tests

## Overview
41 tests covering the Clients CRUD feature: 29 unit tests (controller, service, repository layers) and 12 integration tests (full HTTP pipeline). Uses xUnit 2.9.3 + Moq + Shouldly.

## Location
- `backend/BankOperations.Tests/BankOperations.Tests.csproj` — test project, targets net10.0
- `backend/BankOperations.Tests/Unit/Controllers/Clients/ClientsControllerTests.cs` — 7 unit tests
- `backend/BankOperations.Tests/Unit/Services/Clients/IndividualClientServiceTests.cs` — 6 unit tests
- `backend/BankOperations.Tests/Unit/Services/Clients/CorporateClientServiceTests.cs` — 6 unit tests
- `backend/BankOperations.Tests/Unit/Repositories/Clients/ClientRepositoryTests.cs` — 10 unit tests
- `backend/BankOperations.Tests/Integration/Clients/ClientsIntegrationTests.cs` — 12 integration tests
- `backend/BankOperations/Program.cs` — contains `IsEnvironment("Testing")` branch for InMemory DB + `public partial class Program { }` at the end

## How it works

### Unit tests
Each layer is tested in isolation with Moq mocks for dependencies.

**Controller tests** (`ClientsControllerTests`): mock `IClientService`, `IIndividualClientService`, `ICorporateClientService`. Set up a fake `ClaimsPrincipal` via `ControllerContext` so `User.FindFirstValue(ClaimTypes.NameIdentifier)` works. Assert HTTP result types (`OkObjectResult`, `CreatedAtActionResult`, `NoContentResult`).

**Service tests**: mock `IClientRepository`, `UserManager<ApplicationUser>`, `IPasswordGenerator`, `IEmailService`. Assert that correct exceptions are thrown (ConflictException, NotFoundException) and that repository methods are called with correct arguments.

**Repository tests**: use `DbContextOptionsBuilder().UseInMemoryDatabase(...)` directly. Seed the DbContext, call the repository method, assert the result.

### Integration tests
`BankOperationsWebApplicationFactory` extends `WebApplicationFactory<Program>`:
1. `builder.UseEnvironment("Testing")` — triggers the InMemory DB branch in `Program.cs`
2. `builder.ConfigureAppConfiguration(...)` — injects test JWT secret and dummy email config so `Program.cs` doesn't throw on missing secrets
3. `builder.ConfigureServices(...)` — replaces real `IEmailService` with a Moq mock so `SendWelcomeEmailAsync` doesn't try SMTP

`DataSeeder` runs on host startup and seeds roles (Admin, Employee, Client) into the InMemory DB — required for `UserManager.AddToRoleAsync` to succeed during client creation tests.

JWT tokens are generated in `GenerateJwtToken(userId, role)` using the same secret injected via `ConfigureAppConfiguration`. Claims use full URI types (`ClaimTypes.NameIdentifier`, `ClaimTypes.Role`) which pass through the JWT middleware unchanged.

## Key details

### `public partial class Program { }` — MUST NOT be removed
Top-level statement programs generate an `internal Program` class. `WebApplicationFactory<Program>` in a separate assembly requires `Program` to be accessible. This partial declaration at the end of `Program.cs` makes it `public`.

### InMemory DB branch in Program.cs — MUST NOT be removed
```csharp
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString,
            sqlOptions => sqlOptions.EnableRetryOnFailure(...)));
}
```
This is the only reliable approach. Attempting to remove and re-add the DbContext in `ConfigureServices` causes a dual-provider conflict at runtime (EF Core detects both SqlServer and InMemory).

### JWT claim types in test tokens
Use `ClaimTypes.NameIdentifier` and `ClaimTypes.Role` (full URI form) — NOT short names like `"sub"` or `"role"`. The JWT middleware's `MapInboundClaims` only maps short→long, not long→long, so full URI types pass through as-is and match `TokenValidationParameters.RoleClaimType`.

### xUnit version pinning
- `xunit` → **2.9.3**
- `xunit.runner.visualstudio` → **2.8.2**

`xunit.runner.visualstudio` 3.x requires xUnit v3 (breaking API). 2.8.2 is the last version compatible with xUnit 2.x and C# Dev Kit test discovery.

### MockUserManager helper pattern
`UserManager<T>` has no parameterless constructor. Mock it via:
```csharp
var store = new Mock<IUserStore<ApplicationUser>>();
var userManager = new Mock<UserManager<ApplicationUser>>(
    store.Object, null, null, null, null, null, null, null, null);
```
All constructor parameters after `store` can be `null` for unit tests.

### Test data — unique EGNs and emails per test
The integration tests share one factory instance (`IClassFixture`) and therefore one InMemory DB. Each test that creates a client must use a unique EGN (10 digits) and unique email to avoid cross-test conflicts:
- Test 4: EGN `1111111111`, email `john.doe.test@example.com`
- Test 5 (EGN conflict): EGN `2222222222`
- Test 8 (email conflict): EGN `3333333333`/`3333333334`, email `duplicate.email@example.com`
- Test 9 (update): EGN `4444444444`
- Test 11 (deactivate): EGN `5555555555`

## Code snippets

### WebApplicationFactory setup
```csharp
public class BankOperationsWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "TestSecretKey_AtLeast32CharactersLong!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "TestSecretKey_AtLeast32CharactersLong!",
                ["Jwt:Issuer"] = "bank-operations-api",
                ["Jwt:Audience"] = "bank-operations-client",
                ["Email:SmtpHost"] = "localhost",
                ["Email:SmtpPort"] = "25",
                ["Email:FromEmail"] = "test@test.com",
                ["Email:FromName"] = "Test",
                ["Email:Password"] = "test"
            });
        });

        builder.ConfigureServices(services =>
        {
            var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
            if (emailDescriptor != null) services.Remove(emailDescriptor);

            services.AddScoped<IEmailService>(_ =>
            {
                var mock = new Mock<IEmailService>();
                mock.Setup(e => e.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask);
                mock.Setup(e => e.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask);
                return mock.Object;
            });
        });
    }
}
```

### Controller test setup helper
```csharp
private static ClientsController CreateController(
    IClientService clientService,
    IIndividualClientService individualService,
    ICorporateClientService corporateService,
    Guid userId)
{
    var controller = new ClientsController(clientService, individualService, corporateService);
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, userId.ToString()),
        new("role", "Admin")
    };
    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        }
    };
    return controller;
}
```

### Repository unit test with InMemory DbContext
```csharp
private static ApplicationDbContext CreateContext(string dbName)
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(dbName)
        .Options;
    return new ApplicationDbContext(options);
}
```

## Dependencies
- `xunit` 2.9.3 — test framework
- `xunit.runner.visualstudio` 2.8.2 — VS Code / C# Dev Kit test discovery
- `Moq` 4.20.72 — mocking for unit and integration tests
- `Shouldly` 4.3.0 — fluent assertions (`ShouldBe`, `ShouldBeOfType`)
- `Microsoft.AspNetCore.Mvc.Testing` 10.0.8 — `WebApplicationFactory<Program>`
- `Microsoft.EntityFrameworkCore.InMemory` 10.0.8 — InMemory DB for unit tests and integration tests
- `Microsoft.EntityFrameworkCore.InMemory` also added to main `BankOperations.csproj` — required because `UseInMemoryDatabase` is now called from `Program.cs`

## How to run

### Locally
```bash
dotnet test backend/BankOperations.Tests/BankOperations.Tests.csproj --verbosity normal
```

### With code coverage
```bash
dotnet test backend/BankOperations.Tests/BankOperations.Tests.csproj --collect:"XPlat Code Coverage"
```

### In CI
The `test` job in `.github/workflows/develop_bank-operations-api.yml` runs on every push and PR to `develop`. The `build`, `migrate`, and `deploy` jobs have `if: github.event_name == 'push'` so they are skipped on PRs.

## How to extend

### Add unit tests for a new feature
1. Create `Unit/Controllers/[Feature]/[Feature]ControllerTests.cs`
2. Create `Unit/Services/[Feature]/[Feature]ServiceTests.cs`
3. Create `Unit/Repositories/[Feature]/[Feature]RepositoryTests.cs`
4. Follow Arrange/Act/Assert; mock all dependencies; test happy path + each exception branch

### Add integration tests for a new feature
1. Create `Integration/[Feature]/[Feature]IntegrationTests.cs`
2. Implement `IClassFixture<BankOperationsWebApplicationFactory>`
3. Use `CreateAuthenticatedClient(userId, role)` helper for authenticated requests
4. Use unique identifiers per test to avoid shared-DB interference
