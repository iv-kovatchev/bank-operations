using BankOperations.Data;
using BankOperations.Entities;
using BankOperations.Enums;
using BankOperations.Repositories.BankAccounts;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace BankOperations.Tests.Unit.Repositories.BankAccounts;

public class BankAccountRepositoryTests
{
    private static ApplicationDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static BankAccount MakeAccount(Guid clientId, string iban = "BG99BANK00000000000001") => new()
    {
        ClientId = clientId,
        IBAN = iban,
        Balance = 1000,
        Status = AccountStatus.Active,
        CreatedAt = DateTime.UtcNow,
        CreatedByUserId = Guid.NewGuid()
    };

    [Fact]
    public async Task GetAllByClientIdAsync_ReturnsOnlyClientAccounts()
    {
        // Arrange
        await using var context = CreateDbContext();
        var targetClientId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        // Seed ApplicationUser so Include(ba => ba.CreatedByUser) resolves in InMemory DB
        await context.Users.AddAsync(new ApplicationUser
        {
            Id = createdByUserId,
            Email = "creator@test.com",
            UserName = "creator@test.com",
            FirstName = "Creator",
            LastName = "User"
        });
        await context.SaveChangesAsync();

        await context.BankAccounts.AddRangeAsync(
            new BankAccount { ClientId = targetClientId, IBAN = "BG99BANK00000000000001", Balance = 1000, Status = AccountStatus.Active, CreatedAt = DateTime.UtcNow, CreatedByUserId = createdByUserId },
            new BankAccount { ClientId = targetClientId, IBAN = "BG99BANK00000000000002", Balance = 500, Status = AccountStatus.Active, CreatedAt = DateTime.UtcNow, CreatedByUserId = createdByUserId },
            new BankAccount { ClientId = otherClientId, IBAN = "BG99BANK00000000000003", Balance = 250, Status = AccountStatus.Active, CreatedAt = DateTime.UtcNow, CreatedByUserId = createdByUserId });
        await context.SaveChangesAsync();
        var repository = new BankAccountRepository(context);

        // Act
        var result = await repository.GetAllByClientIdAsync(targetClientId);

        // Assert
        result.Count().ShouldBe(2);
        result.ShouldAllBe(a => a.ClientId == targetClientId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAccount_WhenExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var account = MakeAccount(Guid.NewGuid());
        await context.BankAccounts.AddAsync(account);
        await context.SaveChangesAsync();
        var repository = new BankAccountRepository(context);

        // Act
        var result = await repository.GetByIdAsync(account.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(account.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new BankAccountRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_PersistsAccount()
    {
        // Arrange
        await using var context = CreateDbContext();
        var account = MakeAccount(Guid.NewGuid());
        var repository = new BankAccountRepository(context);

        // Act
        await repository.AddAsync(account);
        await repository.SaveChangesAsync();

        // Assert
        var saved = await context.BankAccounts.FindAsync(account.Id);
        saved.ShouldNotBeNull();
        saved!.IBAN.ShouldBe(account.IBAN);
    }

    [Fact]
    public async Task ExistsByIbanAsync_ReturnsTrue_WhenIbanExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var account = MakeAccount(Guid.NewGuid(), "BG99BANK00000000000001");
        await context.BankAccounts.AddAsync(account);
        await context.SaveChangesAsync();
        var repository = new BankAccountRepository(context);

        // Act
        var result = await repository.ExistsByIbanAsync("BG99BANK00000000000001");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByIbanAsync_ReturnsFalse_WhenIbanNotExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new BankAccountRepository(context);

        // Act
        var result = await repository.ExistsByIbanAsync("BG99BANK00000000000099");

        // Assert
        result.ShouldBeFalse();
    }
}
