using BankOperations.Data;
using BankOperations.Enums;
using BankOperations.Repositories.CreditServices;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using CreditServiceEntity = BankOperations.Entities.CreditService;

namespace BankOperations.Tests.Unit.Repositories.CreditServices;

public class CreditServiceRepositoryTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static CreditServiceEntity CreateCreditService(string name = "Consumer Standard") => new()
    {
        Name = name,
        Type = CreditType.Consumer,
        InterestRate = 5.5m,
        MaxAmount = 10000,
        MaxTermMonths = 36
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllCreditServices()
    {
        // Arrange
        await using var context = CreateDbContext();
        await context.CreditServices.AddRangeAsync(
            CreateCreditService("Consumer Standard"),
            CreateCreditService("Mortgage Standard"));
        await context.SaveChangesAsync();
        var repository = new CreditServiceRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCreditService_WhenFound()
    {
        // Arrange
        await using var context = CreateDbContext();
        var creditService = CreateCreditService();
        await context.CreditServices.AddAsync(creditService);
        await context.SaveChangesAsync();
        var repository = new CreditServiceRepository(context);

        // Act
        var result = await repository.GetByIdAsync(creditService.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(creditService.Id);
        result.Name.ShouldBe(creditService.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditServiceRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_AddsCreditService_AndSaveChangesPersistsIt()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditServiceRepository(context);
        var creditService = CreateCreditService();

        // Act
        await repository.AddAsync(creditService);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.CreditServices.FindAsync(creditService.Id);
        persisted.ShouldNotBeNull();
        persisted!.Name.ShouldBe(creditService.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCreditService_AndSaveChangesPersistsChanges()
    {
        // Arrange
        await using var context = CreateDbContext();
        var creditService = CreateCreditService();
        await context.CreditServices.AddAsync(creditService);
        await context.SaveChangesAsync();
        var repository = new CreditServiceRepository(context);

        // Act
        creditService.Name = "Consumer Updated";
        creditService.InterestRate = 6.5m;
        await repository.UpdateAsync(creditService);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.CreditServices.FindAsync(creditService.Id);
        persisted.ShouldNotBeNull();
        persisted!.Name.ShouldBe("Consumer Updated");
        persisted.InterestRate.ShouldBe(6.5m);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCreditService()
    {
        // Arrange
        await using var context = CreateDbContext();
        var creditService = CreateCreditService();
        await context.CreditServices.AddAsync(creditService);
        await context.SaveChangesAsync();
        var repository = new CreditServiceRepository(context);

        // Act
        await repository.DeleteAsync(creditService.Id);
        await repository.SaveChangesAsync();

        // Assert
        var persisted = await context.CreditServices.FindAsync(creditService.Id);
        persisted.ShouldBeNull();
    }

    [Fact]
    public async Task ExistsByNameAsync_ReturnsTrue_WhenNameExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var creditService = CreateCreditService("Consumer Standard");
        await context.CreditServices.AddAsync(creditService);
        await context.SaveChangesAsync();
        var repository = new CreditServiceRepository(context);

        // Act
        var result = await repository.ExistsByNameAsync("Consumer Standard");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ReturnsFalse_WhenNameDoesNotExist()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new CreditServiceRepository(context);

        // Act
        var result = await repository.ExistsByNameAsync("Nonexistent");

        // Assert
        result.ShouldBeFalse();
    }
}
