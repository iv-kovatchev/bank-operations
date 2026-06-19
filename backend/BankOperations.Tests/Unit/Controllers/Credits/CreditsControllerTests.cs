using System.Security.Claims;
using BankOperations.Controllers;
using BankOperations.DTOs.Credits;
using BankOperations.DTOs.Credits.ConsumerCredits;
using BankOperations.DTOs.Credits.MortgageCredits;
using BankOperations.Enums;
using BankOperations.Exceptions;
using BankOperations.Services.Credits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace BankOperations.Tests.Unit.Controllers.Credits;

public class CreditsControllerTests
{
    private static CreditsController CreateController(ICreditService service, Guid userId, string role)
    {
        var controller = new CreditsController(service);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var serviceMock = new Mock<ICreditService>();
        var credits = new List<CreditResponseDto>
        {
            new() { Id = Guid.NewGuid(), ClientId = clientId, CreditType = "Consumer", Amount = 1000, TermMonths = 12, Status = "Active" }
        };
        serviceMock.Setup(s => s.GetAllByClientIdAsync(clientId, It.IsAny<Guid>(), false)).ReturnsAsync(credits);
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Employee");

        // Act
        var result = await controller.GetAll(clientId);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(credits);
    }

    [Fact]
    public async Task GetAll_ReturnsForbid_WhenClientAccessesAnotherClientsCredits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var serviceMock = new Mock<ICreditService>();
        var controller = CreateController(serviceMock.Object, userId, "Client");

        // Act
        var result = await controller.GetAll(otherClientId);

        // Assert
        result.ShouldBeOfType<ForbidResult>();
        serviceMock.Verify(s => s.GetAllByClientIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenClientAccessesOwnCredits()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var serviceMock = new Mock<ICreditService>();
        var credits = new List<CreditResponseDto>
        {
            new() { Id = Guid.NewGuid(), ClientId = userId, CreditType = "Consumer", Amount = 1000, TermMonths = 12, Status = "Active" }
        };
        serviceMock.Setup(s => s.GetAllByClientIdAsync(userId, userId, false)).ReturnsAsync(credits);
        var controller = CreateController(serviceMock.Object, userId, "Client");

        // Act
        var result = await controller.GetAll(userId);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(credits);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WithDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new CreditResponseDto { Id = id, CreditType = "Consumer", Amount = 1000, TermMonths = 12, Status = "Active" };
        var serviceMock = new Mock<ICreditService>();
        serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<Guid>(), false)).ReturnsAsync(dto);
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Employee");

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
        var serviceMock = new Mock<ICreditService>();
        serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<Guid>(), false)).ThrowsAsync(new NotFoundException("Credit", id));
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Employee");

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => controller.GetById(id));
    }

    [Fact]
    public async Task GrantConsumerCredit_ReturnsCreated_WithDto()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var dto = new CreateConsumerCreditDto
        {
            CreditServiceId = Guid.NewGuid(),
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        var responseDto = new CreditResponseDto
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CreditServiceId = dto.CreditServiceId,
            CreditType = "Consumer",
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Status = "Active",
            Purpose = dto.Purpose.ToString()
        };
        var serviceMock = new Mock<ICreditService>();
        serviceMock
            .Setup(s => s.GrantConsumerCreditAsync(It.IsAny<CreateConsumerCreditDto>(), createdByUserId))
            .ReturnsAsync(responseDto);
        var controller = CreateController(serviceMock.Object, createdByUserId, "Employee");

        // Act
        var result = await controller.GrantConsumerCredit(clientId, dto);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(controller.GetById));
        created.Value.ShouldBe(responseDto);
        dto.ClientId.ShouldBe(clientId);
    }

    [Fact]
    public async Task GrantMortgageCredit_ReturnsCreated_WithDto()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var dto = new CreateMortgageCreditDto
        {
            CreditServiceId = Guid.NewGuid(),
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment
        };
        var responseDto = new CreditResponseDto
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CreditServiceId = dto.CreditServiceId,
            CreditType = "Mortgage",
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Status = "Active",
            PropertyAddress = dto.PropertyAddress,
            PropertyType = dto.PropertyType.ToString()
        };
        var serviceMock = new Mock<ICreditService>();
        serviceMock
            .Setup(s => s.GrantMortgageCreditAsync(It.IsAny<CreateMortgageCreditDto>(), createdByUserId))
            .ReturnsAsync(responseDto);
        var controller = CreateController(serviceMock.Object, createdByUserId, "Employee");

        // Act
        var result = await controller.GrantMortgageCredit(clientId, dto);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(controller.GetById));
        created.Value.ShouldBe(responseDto);
        dto.ClientId.ShouldBe(clientId);
    }

    [Fact]
    public async Task UpdateConsumerCredit_ReturnsOk_WithDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateConsumerCreditDto
        {
            CreditServiceId = Guid.NewGuid(),
            Amount = 2000,
            TermMonths = 24,
            Purpose = CreditPurpose.Education
        };
        var responseDto = new CreditResponseDto
        {
            Id = id,
            CreditServiceId = dto.CreditServiceId,
            CreditType = "Consumer",
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Status = "Active",
            Purpose = dto.Purpose.ToString()
        };
        var serviceMock = new Mock<ICreditService>();
        serviceMock.Setup(s => s.UpdateConsumerCreditAsync(id, dto, It.IsAny<Guid>(), false)).ReturnsAsync(responseDto);
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Employee");

        // Act
        var result = await controller.UpdateConsumerCredit(id, dto);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task UpdateMortgageCredit_ReturnsOk_WithDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateMortgageCreditDto
        {
            CreditServiceId = Guid.NewGuid(),
            Amount = 150000,
            TermMonths = 300,
            PropertyAddress = "2 Main St",
            PropertyType = PropertyType.House
        };
        var responseDto = new CreditResponseDto
        {
            Id = id,
            CreditServiceId = dto.CreditServiceId,
            CreditType = "Mortgage",
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Status = "Active",
            PropertyAddress = dto.PropertyAddress,
            PropertyType = dto.PropertyType.ToString()
        };
        var serviceMock = new Mock<ICreditService>();
        serviceMock.Setup(s => s.UpdateMortgageCreditAsync(id, dto, It.IsAny<Guid>(), false)).ReturnsAsync(responseDto);
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Employee");

        // Act
        var result = await controller.UpdateMortgageCredit(id, dto);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task GetRepaymentPlan_ReturnsOk_WithDto()
    {
        // Arrange
        var creditId = Guid.NewGuid();
        var dto = new RepaymentPlanResponseDto
        {
            CreditId = creditId,
            MonthlyInstallment = 150.25m,
            GeneratedAt = DateTime.UtcNow
        };
        var serviceMock = new Mock<ICreditService>();
        serviceMock.Setup(s => s.GetRepaymentPlanAsync(creditId, It.IsAny<Guid>(), false)).ReturnsAsync(dto);
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Employee");

        // Act
        var result = await controller.GetRepaymentPlan(creditId);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(dto);
    }
}
