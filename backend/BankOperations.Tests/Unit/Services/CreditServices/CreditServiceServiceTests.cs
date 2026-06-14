using BankOperations.DTOs.CreditServices;
using BankOperations.Enums;
using BankOperations.Exceptions;
using BankOperations.Repositories.CreditServices;
using BankOperations.Services.CreditServices;
using Moq;
using Shouldly;
using CreditServiceEntity = BankOperations.Entities.CreditService;

namespace BankOperations.Tests.Unit.Services.CreditServices;

public class CreditServiceServiceTests
{
    private readonly Mock<ICreditServiceRepository> _repoMock = new();

    private CreditServiceService CreateService() => new(_repoMock.Object);

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var creditServices = new List<CreditServiceEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Consumer Standard", Type = CreditType.Consumer, InterestRate = 5.5m, MaxAmount = 10000, MaxTermMonths = 36 },
            new() { Id = Guid.NewGuid(), Name = "Mortgage Standard", Type = CreditType.Mortgage, InterestRate = 3.2m, MaxAmount = 200000, MaxTermMonths = 360 }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(creditServices);

        var service = CreateService();

        // Act
        var result = (await service.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe(creditServices[0].Name);
        result[1].Name.ShouldBe(creditServices[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var creditService = new CreditServiceEntity { Id = id, Name = "Consumer Standard", Type = CreditType.Consumer, InterestRate = 5.5m, MaxAmount = 10000, MaxTermMonths = 36 };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(creditService);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(id);
        result.Name.ShouldBe(creditService.Name);
        result.Type.ShouldBe(CreditType.Consumer.ToString());
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((CreditServiceEntity?)null);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.GetByIdAsync(id));
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsDto()
    {
        // Arrange
        var dto = new CreateCreditServiceDto
        {
            Name = "Consumer Standard",
            Type = CreditType.Consumer,
            InterestRate = 5.5m,
            MaxAmount = 10000,
            MaxTermMonths = 36
        };
        _repoMock.Setup(r => r.ExistsByNameAsync(dto.Name)).ReturnsAsync(false);

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(dto.Name);
        result.Type.ShouldBe(dto.Type.ToString());
        result.InterestRate.ShouldBe(dto.InterestRate);
        result.MaxAmount.ShouldBe(dto.MaxAmount);
        result.MaxTermMonths.ShouldBe(dto.MaxTermMonths);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<CreditServiceEntity>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictException_WhenNameExists()
    {
        // Arrange
        var dto = new CreateCreditServiceDto
        {
            Name = "Consumer Standard",
            Type = CreditType.Consumer,
            InterestRate = 5.5m,
            MaxAmount = 10000,
            MaxTermMonths = 36
        };
        _repoMock.Setup(r => r.ExistsByNameAsync(dto.Name)).ReturnsAsync(true);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var creditService = new CreditServiceEntity { Id = id, Name = "Consumer Standard", Type = CreditType.Consumer, InterestRate = 5.5m, MaxAmount = 10000, MaxTermMonths = 36 };
        var dto = new UpdateCreditServiceDto
        {
            Name = "Consumer Updated",
            Type = CreditType.Consumer,
            InterestRate = 6.0m,
            MaxAmount = 15000,
            MaxTermMonths = 48
        };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(creditService);
        _repoMock.Setup(r => r.ExistsByNameAsync(dto.Name)).ReturnsAsync(false);

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(id, dto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(dto.Name);
        result.InterestRate.ShouldBe(dto.InterestRate);
        result.MaxAmount.ShouldBe(dto.MaxAmount);
        result.MaxTermMonths.ShouldBe(dto.MaxTermMonths);

        _repoMock.Verify(r => r.UpdateAsync(creditService), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCreditServiceDto
        {
            Name = "Consumer Updated",
            Type = CreditType.Consumer,
            InterestRate = 6.0m,
            MaxAmount = 15000,
            MaxTermMonths = 48
        };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((CreditServiceEntity?)null);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsConflictException_WhenNewNameExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var creditService = new CreditServiceEntity { Id = id, Name = "Consumer Standard", Type = CreditType.Consumer, InterestRate = 5.5m, MaxAmount = 10000, MaxTermMonths = 36 };
        var dto = new UpdateCreditServiceDto
        {
            Name = "Mortgage Standard",
            Type = CreditType.Consumer,
            InterestRate = 6.0m,
            MaxAmount = 15000,
            MaxTermMonths = 48
        };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(creditService);
        _repoMock.Setup(r => r.ExistsByNameAsync(dto.Name)).ReturnsAsync(true);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() => service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task DeleteAsync_DeletesSuccessfully_WhenFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var creditService = new CreditServiceEntity { Id = id, Name = "Consumer Standard", Type = CreditType.Consumer, InterestRate = 5.5m, MaxAmount = 10000, MaxTermMonths = 36 };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(creditService);

        var service = CreateService();

        // Act
        await service.DeleteAsync(id);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((CreditServiceEntity?)null);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.DeleteAsync(id));
    }
}
