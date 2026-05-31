using BankOperations.DTOs.Clients.CorporateClients;
using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Exceptions;
using BankOperations.Repositories.Clients;
using BankOperations.Services.Clients.CorporateClients;
using BankOperations.Services.Email;
using BankOperations.Services.Password;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace BankOperations.Tests.Unit.Services.Clients;

public class CorporateClientServiceTests
{
    private readonly Mock<IClientRepository> _repoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock = MockUserManager();
    private readonly Mock<IPasswordGenerator> _passwordGeneratorMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();

    private CorporateClientService CreateService() =>
        new(_repoMock.Object, _userManagerMock.Object, _passwordGeneratorMock.Object, _emailServiceMock.Object);

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            optionsAccessor.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            services.Object,
            logger.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictException_WhenEmailExists()
    {
        // Arrange
        var dto = new CreateCorporateClientDto
        {
            Email = "corp@example.com",
            CompanyName = "Test Corp",
            EIK = "123456789",
            RepresentativeFirstName = "John",
            RepresentativeLastName = "Doe"
        };

        _repoMock.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(true);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() => service.CreateAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflictException_WhenEIKExists()
    {
        // Arrange
        var dto = new CreateCorporateClientDto
        {
            Email = "corp@example.com",
            CompanyName = "Test Corp",
            EIK = "123456789",
            RepresentativeFirstName = "John",
            RepresentativeLastName = "Doe"
        };

        _repoMock.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(false);
        _repoMock.Setup(r => r.ExistsByEIKAsync(dto.EIK)).ReturnsAsync(true);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<ConflictException>(() => service.CreateAsync(dto, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var dto = new CreateCorporateClientDto
        {
            Email = "corp@example.com",
            CompanyName = "Test Corp",
            EIK = "123456789",
            RepresentativeFirstName = "John",
            RepresentativeLastName = "Doe"
        };
        var createdByUserId = Guid.NewGuid();

        _repoMock.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(false);
        _repoMock.Setup(r => r.ExistsByEIKAsync(dto.EIK)).ReturnsAsync(false);
        _passwordGeneratorMock.Setup(p => p.GeneratePassword()).Returns("Test@123456");
        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), "Test@123456"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Client"))
            .ReturnsAsync(IdentityResult.Success);
        _emailServiceMock
            .Setup(e => e.SendWelcomeEmailAsync(dto.Email, dto.RepresentativeFirstName, "Test@123456"))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.CreateAsync(dto, createdByUserId);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<CorporateClientResponseDto>();
        result.CompanyName.ShouldBe(dto.CompanyName);
        result.EIK.ShouldBe(dto.EIK);
        result.RepresentativeFirstName.ShouldBe(dto.RepresentativeFirstName);
        result.RepresentativeLastName.ShouldBe(dto.RepresentativeLastName);
        result.Email.ShouldBe(dto.Email);
        result.CreatedByUserId.ShouldBe(createdByUserId);

        _emailServiceMock.Verify(
            e => e.SendWelcomeEmailAsync(dto.Email, dto.RepresentativeFirstName, "Test@123456"),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenClientNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCorporateClientDto
        {
            CompanyName = "New Corp",
            RepresentativeFirstName = "Jane",
            RepresentativeLastName = "Smith",
            Email = "jane@example.com"
        };

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(id)).ReturnsAsync((Client?)null);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenNotCorporateClient()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCorporateClientDto
        {
            CompanyName = "New Corp",
            RepresentativeFirstName = "Jane",
            RepresentativeLastName = "Smith",
            Email = "jane@example.com"
        };
        var individual = new IndividualClient
        {
            ClientId = id,
            CreatedByUserId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            EGN = "1234567890",
            User = new ApplicationUser { Id = id, Email = "john@example.com", UserName = "john@example.com" }
        };

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(id)).ReturnsAsync(individual);

        var service = CreateService();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => service.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedDto_WhenValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCorporateClientDto
        {
            CompanyName = "Updated Corp",
            RepresentativeFirstName = "Jane",
            RepresentativeLastName = "Smith",
            Email = "jane@example.com"
        };
        var user = new ApplicationUser
        {
            Id = id,
            Email = "corp@example.com",
            UserName = "corp@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var corporate = new CorporateClient
        {
            ClientId = id,
            CreatedByUserId = Guid.NewGuid(),
            CompanyName = "Old Corp",
            EIK = "123456789",
            RepresentativeFirstName = "John",
            RepresentativeLastName = "Doe",
            User = user
        };

        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(id)).ReturnsAsync(corporate);
        _userManagerMock
            .Setup(u => u.SetEmailAsync(user, dto.Email))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(u => u.SetUserNameAsync(user, dto.Email))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(id, dto);

        // Assert
        result.ShouldNotBeNull();
        result.CompanyName.ShouldBe(dto.CompanyName);
        result.RepresentativeFirstName.ShouldBe(dto.RepresentativeFirstName);
        result.RepresentativeLastName.ShouldBe(dto.RepresentativeLastName);
    }
}
