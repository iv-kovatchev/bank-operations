using BankOperations.Controllers;
using BankOperations.DTOs.CreditServices;
using BankOperations.Enums;
using BankOperations.Exceptions;
using BankOperations.Services.CreditServices;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace BankOperations.Tests.Unit.Controllers.CreditServices;

public class CreditServicesControllerTests
{
    private static CreditServicesController CreateController(ICreditServiceService creditServiceService) =>
        new(creditServiceService);

    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        // Arrange
        var serviceMock = new Mock<ICreditServiceService>();
        var creditServices = new List<CreditServiceResponseDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Consumer Standard", Type = CreditType.Consumer.ToString(), InterestRate = 5.5m, MaxAmount = 10000, MaxTermMonths = 36 },
            new() { Id = Guid.NewGuid(), Name = "Mortgage Standard", Type = CreditType.Mortgage.ToString(), InterestRate = 3.2m, MaxAmount = 200000, MaxTermMonths = 360 }
        };
        serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(creditServices);

        var controller = CreateController(serviceMock.Object);

        // Act
        var result = await controller.GetAll();

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(creditServices);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WithDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CreditServiceResponseDto { Id = id, Name = "Consumer Standard", Type = CreditType.Consumer.ToString(), InterestRate = 5.5m, MaxAmount = 10000, MaxTermMonths = 36 };
        var serviceMock = new Mock<ICreditServiceService>();
        serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);

        var controller = CreateController(serviceMock.Object);

        // Act
        var result = await controller.GetById(id);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(dto);
    }

    [Fact]
    public async Task GetById_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var serviceMock = new Mock<ICreditServiceService>();
        serviceMock.Setup(s => s.GetByIdAsync(id)).ThrowsAsync(new NotFoundException("CreditService", id));

        var controller = CreateController(serviceMock.Object);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => controller.GetById(id));
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithDto()
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
        var responseDto = new CreditServiceResponseDto
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Type = dto.Type.ToString(),
            InterestRate = dto.InterestRate,
            MaxAmount = dto.MaxAmount,
            MaxTermMonths = dto.MaxTermMonths
        };
        var serviceMock = new Mock<ICreditServiceService>();
        serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(responseDto);

        var controller = CreateController(serviceMock.Object);

        // Act
        var result = await controller.Create(dto);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(controller.GetById));
        created.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task Create_ThrowsConflictException_WhenNameExists()
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
        var serviceMock = new Mock<ICreditServiceService>();
        serviceMock.Setup(s => s.CreateAsync(dto)).ThrowsAsync(new ConflictException("Credit service with this name already exists."));

        var controller = CreateController(serviceMock.Object);

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() => controller.Create(dto));
    }

    [Fact]
    public async Task Update_ReturnsOk_WithDto()
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
        var responseDto = new CreditServiceResponseDto
        {
            Id = id,
            Name = dto.Name,
            Type = dto.Type.ToString(),
            InterestRate = dto.InterestRate,
            MaxAmount = dto.MaxAmount,
            MaxTermMonths = dto.MaxTermMonths
        };
        var serviceMock = new Mock<ICreditServiceService>();
        serviceMock.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync(responseDto);

        var controller = CreateController(serviceMock.Object);

        // Act
        var result = await controller.Update(id, dto);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task Update_ThrowsNotFoundException_WhenNotFound()
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
        var serviceMock = new Mock<ICreditServiceService>();
        serviceMock.Setup(s => s.UpdateAsync(id, dto)).ThrowsAsync(new NotFoundException("CreditService", id));

        var controller = CreateController(serviceMock.Object);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => controller.Update(id, dto));
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var serviceMock = new Mock<ICreditServiceService>();
        serviceMock.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        var controller = CreateController(serviceMock.Object);

        // Act
        var result = await controller.Delete(id);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var serviceMock = new Mock<ICreditServiceService>();
        serviceMock.Setup(s => s.DeleteAsync(id)).ThrowsAsync(new NotFoundException("CreditService", id));

        var controller = CreateController(serviceMock.Object);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => controller.Delete(id));
    }
}
