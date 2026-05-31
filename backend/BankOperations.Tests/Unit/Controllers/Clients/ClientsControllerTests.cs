using System.Security.Claims;
using BankOperations.Controllers;
using BankOperations.DTOs.Clients.CorporateClients;
using BankOperations.DTOs.Clients.IndividualClients;
using BankOperations.Services.Clients;
using BankOperations.Services.Clients.CorporateClients;
using BankOperations.Services.Clients.IndividualClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace BankOperations.Tests.Unit.Controllers.Clients;

public class ClientsControllerTests
{
    private static ClientsController CreateController(
        IClientService clientService,
        IIndividualClientService individualService,
        ICorporateClientService corporateService,
        Guid userId)
    {
        var controller = new ClientsController(clientService, individualService, corporateService);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("role", "Admin")
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithClientList()
    {
        // Arrange
        var clientService = new Mock<IClientService>();
        var clients = new List<IndividualClientResponseDto>
        {
            new() { Id = Guid.NewGuid(), Email = "alice@test.com", FirstName = "Alice", LastName = "Smith" },
            new() { Id = Guid.NewGuid(), Email = "bob@test.com",   FirstName = "Bob",   LastName = "Jones" }
        };
        clientService.Setup(s => s.GetAllClientsAsync()).ReturnsAsync(clients);

        var controller = CreateController(
            clientService.Object,
            new Mock<IIndividualClientService>().Object,
            new Mock<ICorporateClientService>().Object,
            Guid.NewGuid());

        // Act
        var result = await controller.GetAll();

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(clients);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenClientExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var clientService = new Mock<IClientService>();
        var dto = new IndividualClientResponseDto { Id = id, Email = "client@test.com", FirstName = "John", LastName = "Doe" };
        clientService.Setup(s => s.GetClientByIdAsync(id)).ReturnsAsync(dto);

        var controller = CreateController(
            clientService.Object,
            new Mock<IIndividualClientService>().Object,
            new Mock<ICorporateClientService>().Object,
            Guid.NewGuid());

        // Act
        var result = await controller.GetById(id);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(dto);
    }

    [Fact]
    public async Task CreateIndividual_ReturnsCreated_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var individualService = new Mock<IIndividualClientService>();
        var dto = new CreateIndividualClientDto
        {
            FirstName = "John",
            LastName = "Doe",
            EGN = "1234567890",
            Email = "john.doe@test.com"
        };
        var responseDto = new IndividualClientResponseDto
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EGN = dto.EGN
        };
        individualService.Setup(s => s.CreateAsync(dto, userId)).ReturnsAsync(responseDto);

        var controller = CreateController(
            new Mock<IClientService>().Object,
            individualService.Object,
            new Mock<ICorporateClientService>().Object,
            userId);

        // Act
        var result = await controller.CreateIndividual(dto);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(controller.GetById));
        created.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task CreateCorporate_ReturnsCreated_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var corporateService = new Mock<ICorporateClientService>();
        var dto = new CreateCorporateClientDto
        {
            CompanyName = "Acme Ltd",
            EIK = "123456789",
            RepresentativeFirstName = "Jane",
            RepresentativeLastName = "Smith",
            Email = "acme@test.com"
        };
        var responseDto = new CorporateClientResponseDto
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            CompanyName = dto.CompanyName,
            EIK = dto.EIK,
            RepresentativeFirstName = dto.RepresentativeFirstName,
            RepresentativeLastName = dto.RepresentativeLastName
        };
        corporateService.Setup(s => s.CreateAsync(dto, userId)).ReturnsAsync(responseDto);

        var controller = CreateController(
            new Mock<IClientService>().Object,
            new Mock<IIndividualClientService>().Object,
            corporateService.Object,
            userId);

        // Act
        var result = await controller.CreateCorporate(dto);

        // Assert
        var created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(controller.GetById));
        created.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task UpdateIndividual_ReturnsOk_WhenValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var individualService = new Mock<IIndividualClientService>();
        var dto = new UpdateIndividualClientDto
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@test.com"
        };
        var responseDto = new IndividualClientResponseDto
        {
            Id = id,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };
        individualService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync(responseDto);

        var controller = CreateController(
            new Mock<IClientService>().Object,
            individualService.Object,
            new Mock<ICorporateClientService>().Object,
            Guid.NewGuid());

        // Act
        var result = await controller.UpdateIndividual(id, dto);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task UpdateCorporate_ReturnsOk_WhenValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var corporateService = new Mock<ICorporateClientService>();
        var dto = new UpdateCorporateClientDto
        {
            CompanyName = "NewCorp Ltd",
            RepresentativeFirstName = "Ivan",
            RepresentativeLastName = "Petrov",
            Email = "newcorp@test.com"
        };
        var responseDto = new CorporateClientResponseDto
        {
            Id = id,
            Email = dto.Email,
            CompanyName = dto.CompanyName,
            RepresentativeFirstName = dto.RepresentativeFirstName,
            RepresentativeLastName = dto.RepresentativeLastName
        };
        corporateService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync(responseDto);

        var controller = CreateController(
            new Mock<IClientService>().Object,
            new Mock<IIndividualClientService>().Object,
            corporateService.Object,
            Guid.NewGuid());

        // Act
        var result = await controller.UpdateCorporate(id, dto);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(responseDto);
    }

    [Fact]
    public async Task Deactivate_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var clientService = new Mock<IClientService>();
        clientService.Setup(s => s.DeactivateClientAsync(id)).Returns(Task.CompletedTask);

        var controller = CreateController(
            clientService.Object,
            new Mock<IIndividualClientService>().Object,
            new Mock<ICorporateClientService>().Object,
            Guid.NewGuid());

        // Act
        var result = await controller.Deactivate(id);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }
}
