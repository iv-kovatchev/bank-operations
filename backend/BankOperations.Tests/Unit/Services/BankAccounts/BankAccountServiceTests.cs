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
            User = new ApplicationUser { IsActive = true, FirstName = "Test", LastName = "Client" },
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

    private BankAccount MakeAccountWithClient(Guid accountId, Guid creatorId, decimal balance, AccountStatus status = AccountStatus.Active)
    {
        var account = MakeAccount(Guid.NewGuid());
        account.Id = accountId;
        account.Balance = balance;
        account.Status = status;
        account.Client = MakeClient(account.ClientId);
        account.Client.CreatedByUserId = creatorId;
        return account;
    }

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
        _clientRepoMock.Setup(r => r.GetByIdWithDetailsAsync(clientId)).ReturnsAsync(MakeClient(clientId));
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
        _clientRepoMock.Setup(r => r.GetByIdWithDetailsAsync(clientId)).ReturnsAsync((Client?)null);
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
        _clientRepoMock.Setup(r => r.GetByIdWithDetailsAsync(clientId)).ReturnsAsync(MakeClient(clientId));
        _repoMock.Setup(r => r.ExistsByIbanAsync(dto.IBAN)).ReturnsAsync(true);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() =>
            service.OpenAccountAsync(clientId, dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task OpenAccountAsync_ThrowsValidationException_WhenClientIsInactive()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var dto = new CreateBankAccountDto { IBAN = "BG99BANK00000000000001", InitialBalance = 0 };
        var inactiveClient = MakeClient(clientId);
        inactiveClient.User.IsActive = false;
        _clientRepoMock.Setup(r => r.GetByIdWithDetailsAsync(clientId)).ReturnsAsync(inactiveClient);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() =>
            service.OpenAccountAsync(clientId, dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task CloseAccountAsync_SetsStatusToClosed_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var account = MakeAccount(Guid.NewGuid());
        account.Id = accountId;
        account.Client = MakeClient(account.ClientId);
        account.Client.CreatedByUserId = creatorId;
        BankAccount? captured = null;
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<BankAccount>()))
            .Callback<BankAccount>(a => captured = a)
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        await service.CloseAccountAsync(accountId, creatorId, isAdmin: false);

        // Assert
        captured.ShouldNotBeNull();
        captured!.Status.ShouldBe(AccountStatus.Closed);
    }

    [Fact]
    public async Task CloseAccountAsync_ThrowsNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync((BankAccount?)null);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.CloseAccountAsync(accountId, Guid.NewGuid(), isAdmin: true));
    }

    [Fact]
    public async Task CloseAccountAsync_ThrowsUnauthorizedException_WhenEmployeeDoesNotOwnAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = MakeAccount(Guid.NewGuid());
        account.Id = accountId;
        account.Client = MakeClient(account.ClientId);
        account.Client.CreatedByUserId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedException>(() =>
            service.CloseAccountAsync(accountId, Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task DeleteAccountAsync_SetsIsDeleted_WhenAccountExists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = MakeAccount(Guid.NewGuid());
        account.Id = accountId;
        _repoMock.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync(account);
        _repoMock.Setup(r => r.DeleteAsync(accountId)).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        await service.DeleteAccountAsync(accountId);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_ThrowsNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync((BankAccount?)null);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.DeleteAccountAsync(accountId));
    }

    [Fact]
    public async Task DepositAsync_IncreasesBalance_WhenAccountActive()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var account = MakeAccountWithClient(accountId, creatorId, balance: 1000);
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<BankAccount>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.DepositAsync(accountId, 500, creatorId, isAdmin: false);

        // Assert
        result.Balance.ShouldBe(1500);
    }

    [Fact]
    public async Task DepositAsync_ThrowsNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync((BankAccount?)null);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            service.DepositAsync(accountId, 100, Guid.NewGuid(), isAdmin: true));
    }

    [Fact]
    public async Task DepositAsync_ThrowsUnauthorizedException_WhenEmployeeDoesNotOwnAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = MakeAccountWithClient(accountId, Guid.NewGuid(), balance: 1000);
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedException>(() =>
            service.DepositAsync(accountId, 100, Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task DepositAsync_ThrowsValidationException_WhenAccountClosed()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var account = MakeAccountWithClient(accountId, creatorId, balance: 1000, status: AccountStatus.Closed);
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() =>
            service.DepositAsync(accountId, 100, creatorId, isAdmin: false));
    }

    [Fact]
    public async Task WithdrawAsync_DecreasesBalance_WhenSufficientFunds()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var account = MakeAccountWithClient(accountId, creatorId, balance: 1000);
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<BankAccount>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var service = CreateService();

        // Act
        var result = await service.WithdrawAsync(accountId, 400, creatorId, isAdmin: false);

        // Assert
        result.Balance.ShouldBe(600);
    }

    [Fact]
    public async Task WithdrawAsync_ThrowsNotFoundException_WhenAccountNotFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync((BankAccount?)null);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() =>
            service.WithdrawAsync(accountId, 100, Guid.NewGuid(), isAdmin: true));
    }

    [Fact]
    public async Task WithdrawAsync_ThrowsUnauthorizedException_WhenEmployeeDoesNotOwnAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = MakeAccountWithClient(accountId, Guid.NewGuid(), balance: 1000);
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedException>(() =>
            service.WithdrawAsync(accountId, 100, Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task WithdrawAsync_ThrowsValidationException_WhenAccountClosed()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var account = MakeAccountWithClient(accountId, creatorId, balance: 1000, status: AccountStatus.Closed);
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() =>
            service.WithdrawAsync(accountId, 100, creatorId, isAdmin: false));
    }

    [Fact]
    public async Task WithdrawAsync_ThrowsValidationException_WhenInsufficientFunds()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var account = MakeAccountWithClient(accountId, creatorId, balance: 100);
        _repoMock.Setup(r => r.GetByIdWithClientAsync(accountId)).ReturnsAsync(account);
        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() =>
            service.WithdrawAsync(accountId, 200, creatorId, isAdmin: false));
    }
}
