# Bank Operations System — [CONVENTIONS.md](http://CONVENTIONS.md)

> This file documents exactly how code is written in this project. Update it when a new pattern is introduced. Share with AI at the start of every new chat.

---

## 📁 Project Structure

```
backend/
└── BankOperations/
    ├── Controllers/
    ├── Data/
    │   └── ApplicationDbContext.cs
    ├── Entities/
    │   ├── Clients/
    │   │   ├── Client.cs
    │   │   ├── IndividualClient.cs
    │   │   └── CorporateClient.cs
    │   ├── Credits/
    │   │   ├── Credit.cs
    │   │   ├── ConsumerCredit.cs
    │   │   └── MortgageCredit.cs
    │   ├── BankAccount.cs
    │   ├── CreditService.cs
    │   ├── RepaymentPlan.cs
    │   ├── RepaymentInstallment.cs
    │   └── ActivityLog.cs
    ├── Enums/
    │   ├── CreditPurpose.cs
    │   ├── PropertyType.cs
    │   ├── CreditStatus.cs
    │   └── AccountStatus.cs
    ├── DTOs/
    │   ├── Clients/
    │   ├── Credits/
    │   ├── BankAccounts/
    │   └── Auth/
    ├── Exceptions/
    │   ├── NotFoundException.cs
    │   ├── ValidationException.cs
    │   └── ConflictException.cs
    ├── Repositories/
    │   ├── Base/
    │   │   └── IGenericRepository.cs
    │   ├── Clients/
    │   │   ├── IClientRepository.cs
    │   │   └── ClientRepository.cs
    │   ├── Credits/
    │   │   ├── ICreditRepository.cs
    │   │   └── CreditRepository.cs
    │   └── ...
    └── Services/
        ├── Base/
        │   └── IGenericService.cs
        ├── Clients/
        │   ├── IClientService.cs
        │   └── ClientService.cs
        ├── Credits/
        │   ├── ICreditService.cs
        │   └── CreditService.cs
        └── ...

```

---

## 🔧 Generic Interfaces

Only generic **interfaces** — no generic implementations. Each class writes its own implementation.

```csharp
// Repositories/Base/IGenericRepository.cs
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}

// Services/Base/IGenericService.cs
public interface IGenericService<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

```

---

## 🗄 Repositories

Each repository interface extends the generic interface and adds specific methods. Each repository class implements its own interface directly — no base class.

```csharp
// Repositories/Clients/IClientRepository.cs
public interface IClientRepository : IGenericRepository<Client>
{
    Task<Client?> GetByEGNAsync(string egn);
    Task<Client?> GetByEIKAsync(string eik);
    Task<IEnumerable<Client>> GetAllWithDetailsAsync();
}

// Repositories/Clients/ClientRepository.cs
public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _context;

    public ClientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id)
        => await _context.Clients.FindAsync(id);

    public async Task<IEnumerable<Client>> GetAllAsync()
        => await _context.Clients.ToListAsync();

    public async Task AddAsync(Client entity)
        => await _context.Clients.AddAsync(entity);

    public async Task UpdateAsync(Client entity)
        => _context.Clients.Update(entity);

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) _context.Clients.Remove(entity);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    // Specific methods
    public async Task<Client?> GetByEGNAsync(string egn)
        => await _context.Set<IndividualClient>()
            .FirstOrDefaultAsync(c => c.EGN == egn);
}

```

---

## ⚙️ Services

Same pattern as repositories — interface extends generic, class implements its own interface.

```csharp
// Services/Clients/IClientService.cs
public interface IClientService : IGenericService<Client>
{
    Task<ClientResponseDto> CreateIndividualClientAsync(CreateIndividualClientDto dto, Guid createdByUserId);
    Task<ClientResponseDto> CreateCorporateClientAsync(CreateCorporateClientDto dto, Guid createdByUserId);
}

// Services/Clients/ClientService.cs
public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientResponseDto> CreateIndividualClientAsync(
        CreateIndividualClientDto dto, Guid createdByUserId)
    {
        var existing = await _clientRepository.GetByEGNAsync(dto.EGN);
        if (existing != null)
            throw new ConflictException("Client with this EGN already exists.");

        // One step: create AspNetUsers account with role Client, then create Client record.
        var password = _passwordGenerator.Generate();
        var user = new ApplicationUser { Email = dto.Email, UserName = dto.Email };
        await _userManager.CreateAsync(user, password);
        await _userManager.AddToRoleAsync(user, "Client");

        var client = new IndividualClient
        {
            Id = Guid.Parse(user.Id),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EGN = dto.EGN,
            CreatedByUserId = createdByUserId
        };

        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();

        await _emailService.SendWelcomeEmailAsync(dto.Email, password);

        return new ClientResponseDto { Id = client.Id, Type = "Individual" };
    }
}

```

---

## 🚨 Exceptions

Custom exceptions live in the `Exceptions/` folder. Used in services, caught by global middleware.

```csharp
// Exceptions/NotFoundException.cs
public class NotFoundException : Exception
{
    public NotFoundException(string entity, Guid id)
        : base($"{entity} with id {id} was not found.") { }
}

// Exceptions/ValidationException.cs
public class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message) { }
}

// Exceptions/ConflictException.cs
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message) { }
}

```

Global middleware catches all exceptions and returns structured JSON:

```json
{ "error": "Client with id ... was not found." }

```

---

## 🏗 Entities

### Base entity

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

```

### TPT Inheritance — Clients

`Client.Id` is the PK and simultaneously a FK to `AspNetUsers.Id` (1:1). `IsActive` and `CreatedAt` live on `ApplicationUser` — not duplicated here.

```csharp
public class Client
{
    public Guid Id { get; set; } // PK = FK → AspNetUsers.Id (1:1)
    public ApplicationUser User { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public ICollection<Credit> Credits { get; set; } = new List<Credit>();
}

public class IndividualClient : Client
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EGN { get; set; } = string.Empty;
}

public class CorporateClient : Client
{
    public string CompanyName { get; set; } = string.Empty;
    public string EIK { get; set; } = string.Empty;
    public string RepresentativeFirstName { get; set; } = string.Empty;
    public string RepresentativeLastName { get; set; } = string.Empty;
}

```

### TPT Inheritance — Credits

```csharp
public class Credit : BaseEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid CreditServiceId { get; set; }
    public CreditService CreditService { get; set; } = null!;
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public CreditStatus Status { get; set; } = CreditStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }
    public RepaymentPlan? RepaymentPlan { get; set; }
}

public class ConsumerCredit : Credit
{
    public CreditPurpose Purpose { get; set; }
}

public class MortgageCredit : Credit
{
    public string PropertyAddress { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
}

```

---

## 🌐 Controllers

Thin — only handle HTTP, delegate everything to services.

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpPost("individual")]
    public async Task<IActionResult> CreateIndividual([FromBody] CreateIndividualClientDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _clientService.CreateIndividualClientAsync(dto, userId);
        return Ok(result);
    }
}

```

---

## 📦 DTOs

Always use DTOs — never return entities directly.

```csharp
// DTOs/Clients/CreateIndividualClientDto.cs
// Email is required — used to create the AspNetUsers account in the same transaction.
public class CreateIndividualClientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EGN { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// DTOs/Clients/CreateCorporateClientDto.cs
public class CreateCorporateClientDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string EIK { get; set; } = string.Empty;
    public string RepresentativeFirstName { get; set; } = string.Empty;
    public string RepresentativeLastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// DTOs/Clients/ClientResponseDto.cs
public class ClientResponseDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

```

---

## 🔐 Auth

```csharp
// Get current user ID from JWT in controller
var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

// Role-based authorization
[Authorize(Roles = "Admin")]                  // Admin only
[Authorize(Roles = "Employee,Admin")]         // write operations
[Authorize(Roles = "Client")]                 // client self-service (read-only)
[Authorize(Roles = "Employee,Admin,Client")]  // any authenticated user

```

---

## 📝 Activity Log

Called from services after every operation:

```csharp
await _activityLogService.LogAsync(
    userId: createdByUserId,
    action: "CreateClient",
    entityType: "Client",
    entityId: client.Id,
    details: $"Created individual client with EGN {dto.EGN}"
);

```

---

## 💰 Annuity Repayment Formula

```csharp
decimal monthlyRate = annualInterestRate / 100 / 12;
decimal monthlyInstallment = amount * (monthlyRate * Math.Pow((double)(1 + monthlyRate), termMonths))
                           / (Math.Pow((double)(1 + monthlyRate), termMonths) - 1);

// Per installment
decimal interest = remainingBalance * monthlyRate;
decimal principal = monthlyInstallment - interest;
remainingBalance -= principal;

```

---

## 🧪 Tests

```csharp
public class ClientServiceTests
{
    private readonly Mock<IClientRepository> _mockRepo;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _mockRepo = new Mock<IClientRepository>();
        _service = new ClientService(_mockRepo.Object);
    }

    [Fact]
    public async Task CreateIndividualClient_ShouldReturnDto_WhenValid()
    {
        // Arrange
        // Act
        // Assert
    }
}

```

