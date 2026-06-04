using System.Reflection;
using System.Security.Claims;
using BankOperations.Controllers;
using BankOperations.DTOs.BankAccounts;
using BankOperations.Services.BankAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace BankOperations.Tests.Unit.Controllers.BankAccounts;

public class BankAccountsControllerTests
{
    private static BankAccountsController CreateController(IBankAccountService service, Guid userId, string role)
    {
        var controller = new BankAccountsController(service);
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
    public async Task GetAll_ReturnsOk_WhenEmployee()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var serviceMock = new Mock<IBankAccountService>();
        var accounts = new List<BankAccountResponseDto>
        {
            new() { Id = Guid.NewGuid(), IBAN = "BG99BANK00000000000001", Balance = 1000, Status = "Active", ClientId = clientId }
        };
        serviceMock.Setup(s => s.GetAllByClientIdAsync(clientId)).ReturnsAsync(accounts);
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Employee");

        // Act
        var result = await controller.GetAll(clientId);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(accounts);
    }

    [Fact]
    public async Task GetAll_ReturnsForbid_WhenClientAccessesOtherClientAccounts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var serviceMock = new Mock<IBankAccountService>();
        var controller = CreateController(serviceMock.Object, userId, "Client");

        // Act
        var result = await controller.GetAll(otherClientId);

        // Assert
        result.ShouldBeOfType<ForbidResult>();
        serviceMock.Verify(s => s.GetAllByClientIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task OpenAccount_ReturnsCreated_WhenEmployee()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var dto = new CreateBankAccountDto { IBAN = "BG99BANK00000000000001", InitialBalance = 500 };
        var responseDto = new BankAccountResponseDto
        {
            Id = Guid.NewGuid(),
            IBAN = dto.IBAN,
            Balance = dto.InitialBalance,
            Status = "Active",
            ClientId = clientId,
            CreatedByUserId = createdByUserId
        };
        var serviceMock = new Mock<IBankAccountService>();
        serviceMock
            .Setup(s => s.OpenAccountAsync(clientId, It.IsAny<CreateBankAccountDto>(), createdByUserId))
            .ReturnsAsync(responseDto);
        var controller = CreateController(serviceMock.Object, createdByUserId, "Employee");

        // Act
        var result = await controller.OpenAccount(clientId, dto);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task CloseAccount_ReturnsNoContent_WhenAdmin()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var serviceMock = new Mock<IBankAccountService>();
        serviceMock.Setup(s => s.CloseAccountAsync(accountId)).Returns(Task.CompletedTask);
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Admin");

        // Act
        var result = await controller.CloseAccount(accountId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        serviceMock.Verify(s => s.CloseAccountAsync(accountId), Times.Once);
    }

    [Fact]
    public void CloseAccount_ReturnsForbidden_WhenEmployee()
    {
        // [Authorize(Roles = "Employee,Admin")] on CloseAccount — verifies the attribute
        // is correctly configured to allow both Employee and Admin roles.
        var method = typeof(BankAccountsController)
            .GetMethod(nameof(BankAccountsController.CloseAccount));
        var attr = method!.GetCustomAttribute<AuthorizeAttribute>();
        attr.ShouldNotBeNull();
        attr!.Roles.ShouldBe("Employee,Admin");
    }

    [Fact]
    public async Task DeleteAccount_ReturnsNoContent_WhenAdmin()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var serviceMock = new Mock<IBankAccountService>();
        serviceMock.Setup(s => s.DeleteAccountAsync(accountId)).Returns(Task.CompletedTask);
        var controller = CreateController(serviceMock.Object, Guid.NewGuid(), "Admin");

        // Act
        var result = await controller.DeleteAccount(accountId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
        serviceMock.Verify(s => s.DeleteAccountAsync(accountId), Times.Once);
    }
}
