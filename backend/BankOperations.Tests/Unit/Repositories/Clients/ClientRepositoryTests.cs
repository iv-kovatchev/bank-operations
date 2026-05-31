using BankOperations.Data;
using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Repositories.Clients;
using Shouldly;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Tests.Unit.Repositories.Clients;

public class ClientRepositoryTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IndividualClient CreateIndividualClient(Guid userId)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        return new IndividualClient
        {
            ClientId = userId,
            CreatedByUserId = userId,
            FirstName = "John",
            LastName = "Doe",
            EGN = "1234567890",
            User = user
        };
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsClient_WhenExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var client = CreateIndividualClient(userId);
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.GetByIdAsync(userId);

        // Assert
        result.ShouldNotBeNull();
        result!.ClientId.ShouldBe(userId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllClients()
    {
        // Arrange
        await using var context = CreateDbContext();
        var client1 = CreateIndividualClient(Guid.NewGuid());
        var client2 = CreateIndividualClient(Guid.NewGuid());
        client2.User.Email = "other@example.com";
        client2.User.UserName = "other@example.com";
        client2.EGN = "9876543210";
        await context.Clients.AddRangeAsync(client1, client2);
        await context.SaveChangesAsync();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task AddAsync_AddsClient_AndSavesChanges()
    {
        // Arrange
        await using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var client = CreateIndividualClient(userId);
        var repository = new ClientRepository(context);

        // Act
        await repository.AddAsync(client);
        await repository.SaveChangesAsync();

        // Assert
        var saved = await context.Clients.FindAsync(userId);
        saved.ShouldNotBeNull();
        saved!.ClientId.ShouldBe(userId);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ReturnsTrue_WhenEmailExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var client = CreateIndividualClient(Guid.NewGuid());
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.ExistsByEmailAsync("test@example.com");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ReturnsFalse_WhenEmailNotExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.ExistsByEmailAsync("nonexistent@example.com");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByEGNAsync_ReturnsTrue_WhenEGNExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var client = CreateIndividualClient(Guid.NewGuid());
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.ExistsByEGNAsync("1234567890");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByEGNAsync_ReturnsFalse_WhenEGNNotExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.ExistsByEGNAsync("0000000000");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsByEIKAsync_ReturnsTrue_WhenEIKExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "corp@example.com",
            UserName = "corp@example.com",
            FirstName = "Rep",
            LastName = "Name"
        };
        var client = new CorporateClient
        {
            ClientId = userId,
            CreatedByUserId = userId,
            CompanyName = "Test Corp",
            EIK = "123456789",
            RepresentativeFirstName = "Rep",
            RepresentativeLastName = "Name",
            User = user
        };
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.ExistsByEIKAsync("123456789");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByEIKAsync_ReturnsFalse_WhenEIKNotExists()
    {
        // Arrange
        await using var context = CreateDbContext();
        var repository = new ClientRepository(context);

        // Act
        var result = await repository.ExistsByEIKAsync("000000000");

        // Assert
        result.ShouldBeFalse();
    }
}
