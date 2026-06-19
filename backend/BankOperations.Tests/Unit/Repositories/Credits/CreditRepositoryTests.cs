using BankOperations.Data;
using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Entities.Credits;
using BankOperations.Enums;
using BankOperations.Repositories.Credits;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using CreditServiceEntity = BankOperations.Entities.CreditService;

namespace BankOperations.Tests.Unit.Repositories.Credits;

public class CreditRepositoryTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IndividualClient CreateClient()
    {
        var id = Guid.NewGuid();
        return new IndividualClient
        {
            ClientId = id,
            CreatedByUserId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            EGN = "1234567890",
            User = new ApplicationUser
            {
                Id = id,
                Email = $"{id}@test.com",
                UserName = $"{id}@test.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };
    }

    private static CreditServiceEntity CreateCreditService() => new()
    {
        Name = $"Consumer {Guid.NewGuid()}",
        Type = CreditType.Consumer,
        InterestRate = 5.5m,
        MaxAmount = 10000,
        MaxTermMonths = 36
    };

    private static ConsumerCredit CreateConsumerCredit(Guid clientId, Guid creditServiceId) => new()
    {
        ClientId = clientId,
        CreditServiceId = creditServiceId,
        Amount = 1000,
        TermMonths = 12,
        Status = CreditStatus.Active,
        CreatedByUserId = Guid.NewGuid(),
        Purpose = CreditPurpose.Other
    };

    private static MortgageCredit CreateMortgageCredit(Guid clientId, Guid creditServiceId) => new()
    {
        ClientId = clientId,
        CreditServiceId = creditServiceId,
        Amount = 100000,
        TermMonths = 240,
        Status = CreditStatus.Active,
        CreatedByUserId = Guid.NewGuid(),
        PropertyAddress = "1 Main St",
        PropertyType = PropertyType.Apartment
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllCredits()
    {
        // Arrange
        await using var context = CreateDbContext();
        var clientId = Guid.NewGuid();
        var creditServiceId = Guid.NewGuid();
        await context.Credits.AddRangeAsync(
            CreateConsumerCredit(clientId, creditServiceId),
            CreateConsumerCredit(clientId, creditServiceId));
        await context.SaveChangesAsync();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCredit_WhenFound()
    {
        // Arrange
        await using var context = CreateDbContext();
        var credit = CreateConsumerCredit(Guid.NewGuid(), Guid.NewGuid());
        await context.Credits.AddAsync(credit);
        await context.SaveChangesAsync();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetByIdAsync(credit.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(credit.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_AddsCredit_AndSaveChangesPersistsIt()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditRepository(context);
        var credit = CreateConsumerCredit(Guid.NewGuid(), Guid.NewGuid());

        // Act
        await repository.AddAsync(credit);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.Credits.FindAsync(credit.Id);
        persisted.ShouldNotBeNull();
        persisted!.Amount.ShouldBe(credit.Amount);
    }

    [Fact]
    public async Task AddAsync_AddsMortgageCredit_AndSaveChangesPersistsIt()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditRepository(context);
        var credit = CreateMortgageCredit(Guid.NewGuid(), Guid.NewGuid());

        // Act
        await repository.AddAsync(credit);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.MortgageCredits.FindAsync(credit.Id);
        persisted.ShouldNotBeNull();
        persisted!.Amount.ShouldBe(credit.Amount);
        persisted.PropertyAddress.ShouldBe(credit.PropertyAddress);
    }

    [Fact]
    public async Task GetAllByClientIdAsync_ReturnsCredits_ForClient()
    {
        // Arrange
        await using var context = CreateDbContext();
        var targetClient = CreateClient();
        var otherClient = CreateClient();
        var creditService = CreateCreditService();
        await context.Clients.AddRangeAsync(targetClient, otherClient);
        await context.CreditServices.AddAsync(creditService);
        await context.SaveChangesAsync();

        await context.Credits.AddRangeAsync(
            CreateConsumerCredit(targetClient.ClientId, creditService.Id),
            CreateConsumerCredit(targetClient.ClientId, creditService.Id),
            CreateConsumerCredit(otherClient.ClientId, creditService.Id));
        await context.SaveChangesAsync();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetAllByClientIdAsync(targetClient.ClientId);

        // Assert
        result.Count().ShouldBe(2);
        result.ShouldAllBe(c => c.ClientId == targetClient.ClientId);
    }

    [Fact]
    public async Task GetAllByClientIdAsync_ReturnsEmpty_WhenNoCreditsForClient()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetAllByClientIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ReturnsCredit_WithNavigations()
    {
        // Arrange
        await using var context = CreateDbContext();
        var client = CreateClient();
        var creditService = CreateCreditService();
        await context.Clients.AddAsync(client);
        await context.CreditServices.AddAsync(creditService);
        await context.SaveChangesAsync();

        var credit = CreateConsumerCredit(client.ClientId, creditService.Id);
        await context.Credits.AddAsync(credit);
        await context.SaveChangesAsync();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(credit.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Client.ShouldNotBeNull();
        result.Client.ClientId.ShouldBe(client.ClientId);
        result.CreditService.ShouldNotBeNull();
        result.CreditService.Id.ShouldBe(creditService.Id);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ReturnsMortgageCredit_WithNavigations()
    {
        // Arrange
        await using var context = CreateDbContext();
        var client = CreateClient();
        var creditService = CreateCreditService();
        await context.Clients.AddAsync(client);
        await context.CreditServices.AddAsync(creditService);
        await context.SaveChangesAsync();

        var credit = CreateMortgageCredit(client.ClientId, creditService.Id);
        await context.Credits.AddAsync(credit);
        await context.SaveChangesAsync();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(credit.Id);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<MortgageCredit>();
        result!.Client.ShouldNotBeNull();
        result.Client.ClientId.ShouldBe(client.ClientId);
        result.CreditService.ShouldNotBeNull();
        result.CreditService.Id.ShouldBe(creditService.Id);
        ((MortgageCredit)result).PropertyAddress.ShouldBe(credit.PropertyAddress);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetByIdWithDetailsAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetRepaymentPlanAsync_ReturnsPlan_WithInstallments()
    {
        // Arrange
        await using var context = CreateDbContext();
        var credit = CreateConsumerCredit(Guid.NewGuid(), Guid.NewGuid());
        await context.Credits.AddAsync(credit);
        await context.SaveChangesAsync();

        var plan = new RepaymentPlan
        {
            CreditId = credit.Id,
            MonthlyInstallment = 100,
            Installments = new List<RepaymentInstallment>
            {
                new() { InstallmentNumber = 1, DueDate = DateTime.UtcNow, PrincipalPart = 90, InterestPart = 10, RemainingBalance = 910 },
                new() { InstallmentNumber = 2, DueDate = DateTime.UtcNow, PrincipalPart = 91, InterestPart = 9, RemainingBalance = 819 }
            }
        };
        await context.RepaymentPlans.AddAsync(plan);
        await context.SaveChangesAsync();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetRepaymentPlanAsync(credit.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.CreditId.ShouldBe(credit.Id);
        result.Installments.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetRepaymentPlanAsync_ReturnsNull_WhenNoPlanForCredit()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditRepository(context);

        // Act
        var result = await repository.GetRepaymentPlanAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }
}
