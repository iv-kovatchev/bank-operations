using BankOperations.DTOs.BankAccounts;
using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Enums;
using BankOperations.Exceptions;
using BankOperations.Repositories.BankAccounts;
using BankOperations.Repositories.Clients;
using BankOperations.Services.BankAccounts;
using Moq;
using Shouldly;

namespace BankOperations.Tests.Unit.Services.BankAccounts;

public class BankAccountServiceTests
{
    private readonly Mock<IBankAccountRepository> _repoMock = new();
    private readonly Mock<IClientRepository> _clientRepoMock = new();

    private BankAccountService CreateService() =>
        new(_repoMock.Object, _clientRepoMock.Object);

    private static Client MakeClient(Guid clientId) =>
        new IndividualClient
        {
            ClientId = clientId,
            CreatedByUserId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Client",
            EGN = "1234567890"
        };

    private static BankAccount MakeAccount(Guid clientId) => new()
    {
        ClientId = clientId,
        IBAN = "BG99BANK00000000000001",
        Balance = 1000,
        Status = AccountStatus.Active,
        CreatedAt = DateTime.UtcNow,
        CreatedByUserId = Guid.NewGuid()
    };

    [Fact]
    public async Task GetAllByClientIdAsync_ReturnsAccounts_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var accounts = new List<BankAccount> { MakeAccount(clientId) };
        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(MakeClient(clientId));
        _repoMock.Setup(r => r.GetAllByClientIdAsync(clientId)).ReturnsAsync(accounts);
        var service = CreateService();

        // Act
        var result = await service.GetAllByClientIdAsync(clientId);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
    }

    [Fact]
    public async Task GetAllByClientIdAsync_ThrowsNotFoundException_WhenClientNotFound()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync((Client?)null);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.GetAllByClientIdAsync(clientId));
    }

    [Fact]
    public async Task OpenAccountAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var dto = new CreateBankAccountDto { IBAN = "BG99BANK00000000000001", InitialBalance = 500 };
        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(MakeClient(clientId));
        _repoMock.Setup(r => r.ExistsByIbanAsync(dto.IBAN)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<BankAccount>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.OpenAccountAsync(clientId, dto, createdByUserId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<BankAccountResponseDto>();
        result.IBAN.ShouldBe(dto.IBAN);
        result.Balance.ShouldBe(dto.InitialBalance);
        result.Status.ShouldBe("Active");
        result.ClientId.ShouldBe(clientId);
        result.CreatedByUserId.ShouldBe(createdByUserId);
    }

    [Fact]
    public async Task OpenAccountAsync_ThrowsNotFoundException_WhenClientNotFound()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync((Client?)null);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            service.OpenAccountAsync(clientId, new CreateBankAccountDto(), Guid.NewGuid()));
    }

    [Fact]
    public async Task OpenAccountAsync_ThrowsConflictException_WhenIbanExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var dto = new CreateBankAccountDto { IBAN = "BG99BANK00000000000001", InitialBalance = 0 };
        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync(MakeClient(clientId));
        _repoMock.Setup(r => r.ExistsByIbanAsync(dto.IBAN)).ReturnsAsync(true);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() =>
            service.OpenAccountAsync(clientId, dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task CloseAccountAsync_SetsStatusToClosed_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = MakeAccount(Guid.NewGuid());
        account.Id = accountId;
        BankAccount? captured = null;
        _repoMock.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync(account);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<BankAccount>()))
            .Callback<BankAccount>(a => captured = a)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        await service.CloseAccountAsync(accountId);

        // Assert
        captured.ShouldNotBeNull();
        captured!.Status.ShouldBe(AccountStatus.Closed);
    }

    [Fact]
    public async Task CloseAccountAsync_ThrowsNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync((BankAccount?)null);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.CloseAccountAsync(accountId));
    }
}
