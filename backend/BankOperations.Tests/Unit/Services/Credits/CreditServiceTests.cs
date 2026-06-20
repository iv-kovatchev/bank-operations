using BankOperations.DTOs.Credits.ConsumerCredits;
using BankOperations.DTOs.Credits.MortgageCredits;
using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Entities.Credits;
using BankOperations.Enums;
using BankOperations.Exceptions;
using BankOperations.Repositories.BankAccounts;
using BankOperations.Repositories.Clients;
using BankOperations.Repositories.Credits;
using BankOperations.Repositories.CreditServices;
using Moq;
using Shouldly;
using CreditServiceEntity = BankOperations.Entities.CreditService;

namespace BankOperations.Tests.Unit.Services.Credits;

public class CreditServiceTests
{
    private readonly Mock<ICreditRepository> _creditRepoMock = new();
    private readonly Mock<ICreditServiceRepository> _creditServiceRepoMock = new();
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<IBankAccountRepository> _bankAccountRepoMock = new();

    private BankOperations.Services.Credits.CreditService CreateService() =>
        new(_creditRepoMock.Object, _creditServiceRepoMock.Object, _clientRepoMock.Object, _bankAccountRepoMock.Object);

    private static CreditServiceEntity MakeCreditService(decimal maxAmount = 10000, int maxTermMonths = 36) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Consumer Standard",
        Type = CreditType.Consumer,
        InterestRate = 5.5m,
        MaxAmount = maxAmount,
        MaxTermMonths = maxTermMonths
    };

    private static Client MakeClient(Guid createdByUserId, bool isActive = true) => new()
    {
        ClientId = Guid.NewGuid(),
        CreatedByUserId = createdByUserId,
        User = new ApplicationUser { IsActive = isActive }
    };

    [Fact]
    public async Task GetAllByClientIdAsync_ReturnsCredits_ForClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var employee1 = Guid.NewGuid();
        var employee2 = Guid.NewGuid();
        var credits = new List<Credit>
        {
            new ConsumerCredit { Id = Guid.NewGuid(), ClientId = clientId, Amount = 1000, TermMonths = 12, Purpose = CreditPurpose.Other, Client = MakeClient(employee1) },
            new ConsumerCredit { Id = Guid.NewGuid(), ClientId = clientId, Amount = 2000, TermMonths = 24, Purpose = CreditPurpose.Other, Client = MakeClient(employee2) }
        };
        _creditRepoMock.Setup(r => r.GetAllByClientIdAsync(clientId)).ReturnsAsync(credits);

        var service = CreateService();

        // Act
        var result = (await service.GetAllByClientIdAsync(clientId, employee1, isAdmin: true)).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllByClientIdAsync_FiltersCredits_WhenEmployee()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        var ownCredit = new ConsumerCredit { Id = Guid.NewGuid(), ClientId = clientId, Amount = 1000, TermMonths = 12, Purpose = CreditPurpose.Other, Client = MakeClient(employeeId) };
        var otherCredit = new ConsumerCredit { Id = Guid.NewGuid(), ClientId = clientId, Amount = 2000, TermMonths = 24, Purpose = CreditPurpose.Other, Client = MakeClient(otherEmployeeId) };
        _creditRepoMock.Setup(r => r.GetAllByClientIdAsync(clientId)).ReturnsAsync(new List<Credit> { ownCredit, otherCredit });

        var service = CreateService();

        // Act
        var result = (await service.GetAllByClientIdAsync(clientId, employeeId, isAdmin: false)).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(ownCredit.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var credit = new ConsumerCredit { Id = Guid.NewGuid(), Amount = 1000, TermMonths = 12, Purpose = CreditPurpose.Other, Client = MakeClient(employeeId) };
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(credit.Id)).ReturnsAsync(credit);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(credit.Id, employeeId, isAdmin: false);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(credit.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(id)).ReturnsAsync((Credit?)null);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.GetByIdAsync(id, Guid.NewGuid(), false));
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsUnauthorizedException_WhenEmployeeAccessesAnotherEmployeesCredit()
    {
        // Arrange
        var ownerEmployeeId = Guid.NewGuid();
        var requestingEmployeeId = Guid.NewGuid();
        var credit = new ConsumerCredit { Id = Guid.NewGuid(), Amount = 1000, TermMonths = 12, Purpose = CreditPurpose.Other, Client = MakeClient(ownerEmployeeId) };
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(credit.Id)).ReturnsAsync(credit);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<UnauthorizedException>(() => service.GetByIdAsync(credit.Id, requestingEmployeeId, isAdmin: false));
    }

    [Fact]
    public async Task GrantConsumerCreditAsync_GrantsCredit_AndGeneratesRepaymentPlan()
    {
        // Arrange
        var createdByUserId = Guid.NewGuid();
        var creditService = MakeCreditService();
        var client = MakeClient(createdByUserId);
        var dto = new CreateConsumerCreditDto
        {
            ClientId = client.ClientId,
            CreditServiceId = creditService.Id,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };

        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(creditService.Id)).ReturnsAsync(creditService);
        _clientRepoMock.Setup(r => r.GetByIdWithDetailsAsync(client.ClientId)).ReturnsAsync(client);

        var service = CreateService();

        // Act
        var result = await service.GrantConsumerCreditAsync(dto, createdByUserId);

        // Assert
        result.ShouldNotBeNull();
        result.ClientId.ShouldBe(dto.ClientId);
        result.Amount.ShouldBe(dto.Amount);
        result.CreditType.ShouldBe("Consumer");

        _creditRepoMock.Verify(r => r.AddAsync(It.IsAny<ConsumerCredit>()), Times.Once);
        _creditRepoMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
        _creditRepoMock.Verify(r => r.AddRepaymentPlanAsync(It.IsAny<RepaymentPlan>()), Times.Once);
    }

    [Fact]
    public async Task GrantConsumerCreditAsync_ThrowsNotFoundException_WhenCreditServiceNotFound()
    {
        // Arrange
        var dto = new CreateConsumerCreditDto
        {
            ClientId = Guid.NewGuid(),
            CreditServiceId = Guid.NewGuid(),
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(dto.CreditServiceId)).ReturnsAsync((CreditServiceEntity?)null);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.GrantConsumerCreditAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task GrantConsumerCreditAsync_ThrowsValidationException_WhenAmountExceedsMax()
    {
        // Arrange
        var creditService = MakeCreditService(maxAmount: 500);
        var dto = new CreateConsumerCreditDto
        {
            ClientId = Guid.NewGuid(),
            CreditServiceId = creditService.Id,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(creditService.Id)).ReturnsAsync(creditService);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.GrantConsumerCreditAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task GrantConsumerCreditAsync_ThrowsValidationException_WhenTermExceedsMax()
    {
        // Arrange
        var creditService = MakeCreditService(maxTermMonths: 6);
        var dto = new CreateConsumerCreditDto
        {
            ClientId = Guid.NewGuid(),
            CreditServiceId = creditService.Id,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(creditService.Id)).ReturnsAsync(creditService);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.GrantConsumerCreditAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task GrantConsumerCreditAsync_ThrowsValidationException_WhenClientIsInactive()
    {
        // Arrange
        var creditService = MakeCreditService();
        var client = MakeClient(Guid.NewGuid(), isActive: false);
        var dto = new CreateConsumerCreditDto
        {
            ClientId = client.ClientId,
            CreditServiceId = creditService.Id,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(creditService.Id)).ReturnsAsync(creditService);
        _clientRepoMock.Setup(r => r.GetByIdWithDetailsAsync(client.ClientId)).ReturnsAsync(client);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.GrantConsumerCreditAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task GrantMortgageCreditAsync_GrantsCredit_AndGeneratesRepaymentPlan()
    {
        // Arrange
        var createdByUserId = Guid.NewGuid();
        var creditService = MakeCreditService(maxAmount: 200000, maxTermMonths: 360);
        var client = MakeClient(createdByUserId);
        var dto = new CreateMortgageCreditDto
        {
            ClientId = client.ClientId,
            CreditServiceId = creditService.Id,
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment
        };

        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(creditService.Id)).ReturnsAsync(creditService);
        _clientRepoMock.Setup(r => r.GetByIdWithDetailsAsync(client.ClientId)).ReturnsAsync(client);

        var service = CreateService();

        // Act
        var result = await service.GrantMortgageCreditAsync(dto, createdByUserId);

        // Assert
        result.ShouldNotBeNull();
        result.ClientId.ShouldBe(dto.ClientId);
        result.Amount.ShouldBe(dto.Amount);
        result.CreditType.ShouldBe("Mortgage");
        result.PropertyAddress.ShouldBe(dto.PropertyAddress);

        _creditRepoMock.Verify(r => r.AddAsync(It.IsAny<MortgageCredit>()), Times.Once);
        _creditRepoMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
        _creditRepoMock.Verify(r => r.AddRepaymentPlanAsync(It.IsAny<RepaymentPlan>()), Times.Once);
    }

    [Fact]
    public async Task GrantMortgageCreditAsync_ThrowsNotFoundException_WhenCreditServiceNotFound()
    {
        // Arrange
        var dto = new CreateMortgageCreditDto
        {
            ClientId = Guid.NewGuid(),
            CreditServiceId = Guid.NewGuid(),
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment
        };
        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(dto.CreditServiceId)).ReturnsAsync((CreditServiceEntity?)null);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.GrantMortgageCreditAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task GrantMortgageCreditAsync_ThrowsValidationException_WhenAmountExceedsMax()
    {
        // Arrange
        var creditService = MakeCreditService(maxAmount: 50000);
        var dto = new CreateMortgageCreditDto
        {
            ClientId = Guid.NewGuid(),
            CreditServiceId = creditService.Id,
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment
        };
        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(creditService.Id)).ReturnsAsync(creditService);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.GrantMortgageCreditAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task GrantMortgageCreditAsync_ThrowsValidationException_WhenTermExceedsMax()
    {
        // Arrange
        var creditService = MakeCreditService(maxTermMonths: 120);
        var dto = new CreateMortgageCreditDto
        {
            ClientId = Guid.NewGuid(),
            CreditServiceId = creditService.Id,
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment
        };
        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(creditService.Id)).ReturnsAsync(creditService);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.GrantMortgageCreditAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task GrantMortgageCreditAsync_ThrowsValidationException_WhenClientIsInactive()
    {
        // Arrange
        var creditService = MakeCreditService();
        var client = MakeClient(Guid.NewGuid(), isActive: false);
        var dto = new CreateMortgageCreditDto
        {
            ClientId = client.ClientId,
            CreditServiceId = creditService.Id,
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment
        };
        _creditServiceRepoMock.Setup(r => r.GetByIdAsync(creditService.Id)).ReturnsAsync(creditService);
        _clientRepoMock.Setup(r => r.GetByIdWithDetailsAsync(client.ClientId)).ReturnsAsync(client);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.GrantMortgageCreditAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateMortgageCreditAsync_ThrowsValidationException_WhenCreditHasPaidInstallments()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var creditServiceEntity = MakeCreditService();
        var credit = new MortgageCredit
        {
            Id = Guid.NewGuid(),
            Status = CreditStatus.Active,
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment,
            CreditServiceId = creditServiceEntity.Id,
            CreditService = creditServiceEntity,
            Client = MakeClient(employeeId),
            RepaymentPlan = new RepaymentPlan
            {
                Installments = new List<RepaymentInstallment>
                {
                    new() { PaidAt = DateTime.UtcNow }
                }
            }
        };
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(credit.Id)).ReturnsAsync(credit);

        var dto = new UpdateMortgageCreditDto
        {
            CreditServiceId = creditServiceEntity.Id,
            Amount = 150000,
            TermMonths = 300,
            PropertyAddress = "2 Main St",
            PropertyType = PropertyType.House
        };

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.UpdateMortgageCreditAsync(credit.Id, dto, employeeId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateMortgageCreditAsync_ThrowsValidationException_WhenCreditIsNotActive()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var creditServiceEntity = MakeCreditService();
        var credit = new MortgageCredit
        {
            Id = Guid.NewGuid(),
            Status = CreditStatus.PaidOff,
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment,
            CreditServiceId = creditServiceEntity.Id,
            CreditService = creditServiceEntity,
            Client = MakeClient(employeeId)
        };
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(credit.Id)).ReturnsAsync(credit);

        var dto = new UpdateMortgageCreditDto
        {
            CreditServiceId = creditServiceEntity.Id,
            Amount = 150000,
            TermMonths = 300,
            PropertyAddress = "2 Main St",
            PropertyType = PropertyType.House
        };

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.UpdateMortgageCreditAsync(credit.Id, dto, employeeId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateConsumerCreditAsync_ThrowsValidationException_WhenCreditHasPaidInstallments()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var creditServiceEntity = MakeCreditService();
        var credit = new ConsumerCredit
        {
            Id = Guid.NewGuid(),
            Status = CreditStatus.Active,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other,
            CreditServiceId = creditServiceEntity.Id,
            CreditService = creditServiceEntity,
            Client = MakeClient(employeeId),
            RepaymentPlan = new RepaymentPlan
            {
                Installments = new List<RepaymentInstallment>
                {
                    new() { PaidAt = DateTime.UtcNow }
                }
            }
        };
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(credit.Id)).ReturnsAsync(credit);

        var dto = new UpdateConsumerCreditDto
        {
            CreditServiceId = creditServiceEntity.Id,
            Amount = 2000,
            TermMonths = 24,
            Purpose = CreditPurpose.Other
        };

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.UpdateConsumerCreditAsync(credit.Id, dto, employeeId, isAdmin: false));
    }

    [Fact]
    public async Task UpdateConsumerCreditAsync_ThrowsValidationException_WhenCreditIsNotActive()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var creditServiceEntity = MakeCreditService();
        var credit = new ConsumerCredit
        {
            Id = Guid.NewGuid(),
            Status = CreditStatus.PaidOff,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other,
            CreditServiceId = creditServiceEntity.Id,
            CreditService = creditServiceEntity,
            Client = MakeClient(employeeId)
        };
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(credit.Id)).ReturnsAsync(credit);

        var dto = new UpdateConsumerCreditDto
        {
            CreditServiceId = creditServiceEntity.Id,
            Amount = 2000,
            TermMonths = 24,
            Purpose = CreditPurpose.Other
        };

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ValidationException>(() => service.UpdateConsumerCreditAsync(credit.Id, dto, employeeId, isAdmin: false));
    }

    [Fact]
    public async Task GetRepaymentPlanAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var credit = new ConsumerCredit
        {
            Id = Guid.NewGuid(),
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other,
            Client = MakeClient(employeeId),
            RepaymentPlan = new RepaymentPlan
            {
                CreditId = Guid.NewGuid(),
                MonthlyInstallment = 100,
                Installments = new List<RepaymentInstallment>()
            }
        };
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(credit.Id)).ReturnsAsync(credit);

        var service = CreateService();

        // Act
        var result = await service.GetRepaymentPlanAsync(credit.Id, employeeId, isAdmin: false);

        // Assert
        result.ShouldNotBeNull();
        result.MonthlyInstallment.ShouldBe(100);
    }

    [Fact]
    public async Task GetRepaymentPlanAsync_ThrowsNotFoundException_WhenPlanIsNull()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var credit = new ConsumerCredit
        {
            Id = Guid.NewGuid(),
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other,
            Client = MakeClient(employeeId),
            RepaymentPlan = null
        };
        _creditRepoMock.Setup(r => r.GetByIdWithDetailsAsync(credit.Id)).ReturnsAsync(credit);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.GetRepaymentPlanAsync(credit.Id, employeeId, isAdmin: false));
    }
}
