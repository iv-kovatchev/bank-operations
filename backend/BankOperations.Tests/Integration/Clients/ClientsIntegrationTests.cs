using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BankOperations.Data;
using BankOperations.DTOs.Clients.IndividualClients;
using BankOperations.Services.Email;
using Moq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shouldly;

namespace BankOperations.Tests.Integration.Clients;

// File-scoped no-op: prevents real SMTP calls during tests
file sealed class NoOpEmailService : IEmailService
{
    public Task SendOtpEmailAsync(string toEmail, string firstName, string otpCode) => Task.CompletedTask;
    public Task SendWelcomeEmailAsync(string toEmail, string firstName, string password) => Task.CompletedTask;
}

public class BankOperationsWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "TestSecretKey_AtLeast32CharactersLong!";
    private const string TestIssuer = "bank-operations-api";
    private const string TestAudience = "bank-operations-client";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "TestSecretKey_AtLeast32CharactersLong!",
                ["Jwt:Issuer"] = "bank-operations-api",
                ["Jwt:Audience"] = "bank-operations-client",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["Email:SmtpHost"] = "localhost",
                ["Email:SmtpPort"] = "25",
                ["Email:FromEmail"] = "test@test.com",
                ["Email:FromName"] = "Test",
                ["Email:Password"] = "test"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real EmailService
            var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
            if (emailDescriptor != null)
                services.Remove(emailDescriptor);

            // Add mock EmailService that does nothing
            services.AddScoped<IEmailService>(_ =>
            {
                var mock = new Mock<IEmailService>();
                mock.Setup(e => e.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask);
                mock.Setup(e => e.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.CompletedTask);
                return mock.Object;
            });
        });
    }

    public string GenerateJwtToken(Guid userId, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Use full URI claim types — JWT middleware passes them through unchanged (no remapping of URIs)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class ClientsIntegrationTests : IClassFixture<BankOperationsWebApplicationFactory>
{
    private readonly BankOperationsWebApplicationFactory _factory;
    private static readonly Guid AdminUserId = Guid.NewGuid();
    private static readonly Guid ClientUserId = Guid.NewGuid();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ClientsIntegrationTests(BankOperationsWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateUnauthenticatedClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private HttpClient CreateAuthenticatedClient(Guid userId, string role)
    {
        var token = _factory.GenerateJwtToken(userId, role);
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static StringContent JsonBody(object dto) =>
        new(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

    [Fact]
    public async Task GetAll_Returns401_WhenNotAuthenticated()
    {
        // Arrange
        var client = CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/clients");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_Returns403_WhenClientRole()
    {
        // Arrange
        var client = CreateAuthenticatedClient(ClientUserId, "Client");

        // Act
        var response = await client.GetAsync("/api/clients");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_Returns200_WhenAdminRole()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");

        // Act
        var response = await client.GetAsync("/api/clients");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateIndividual_Returns201_WhenValid()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");
        var dto = new CreateIndividualClientDto
        {
            FirstName = "John",
            LastName = "Doe",
            EGN = "1111111111",
            Email = "john.doe.test@example.com"
        };

        // Act
        var response = await client.PostAsync("/api/clients/individual", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateIndividual_Returns409_WhenEGNExists()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");
        var firstDto = new CreateIndividualClientDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            EGN = "2222222222",
            Email = "jane.smith.first@example.com"
        };
        var duplicateDto = new CreateIndividualClientDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            EGN = "2222222222",
            Email = "jane.smith.second@example.com"
        };

        // Act — seed the first client, then try with the same EGN
        await client.PostAsync("/api/clients/individual", JsonBody(firstDto));
        var response = await client.PostAsync("/api/clients/individual", JsonBody(duplicateDto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotExists()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/clients/{nonExistentId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateIndividual_Returns400_WhenInvalidDto()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");
        var dto = new CreateIndividualClientDto
        {
            FirstName = "John",
            LastName = "Doe",
            EGN = "",
            Email = "john.invalid@example.com"
        };

        // Act
        var response = await client.PostAsync("/api/clients/individual", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateIndividual_Returns409_WhenEmailExists()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");
        var firstDto = new CreateIndividualClientDto
        {
            FirstName = "Alice",
            LastName = "Brown",
            EGN = "3333333333",
            Email = "duplicate.email@example.com"
        };
        var duplicateDto = new CreateIndividualClientDto
        {
            FirstName = "Alice",
            LastName = "Brown",
            EGN = "3333333334",
            Email = "duplicate.email@example.com"
        };

        // Act — seed the first client, then try with the same email
        await client.PostAsync("/api/clients/individual", JsonBody(firstDto));
        var response = await client.PostAsync("/api/clients/individual", JsonBody(duplicateDto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateIndividual_Returns200_WhenValid()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");
        var createDto = new CreateIndividualClientDto
        {
            FirstName = "Bob",
            LastName = "Green",
            EGN = "4444444444",
            Email = "bob.green.update@example.com"
        };
        var createResponse = await client.PostAsync("/api/clients/individual", JsonBody(createDto));
        var body = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<IndividualClientResponseDto>(body, JsonOptions);
        var updateDto = new UpdateIndividualClientDto
        {
            FirstName = "Robert",
            LastName = "Green",
            Email = "bob.green.updated@example.com"
        };

        // Act
        var response = await client.PutAsync($"/api/clients/individual/{created!.Id}", JsonBody(updateDto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateIndividual_Returns404_WhenNotExists()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");
        var nonExistentId = Guid.NewGuid();
        var dto = new UpdateIndividualClientDto
        {
            FirstName = "Ghost",
            LastName = "User",
            Email = "ghost@example.com"
        };

        // Act
        var response = await client.PutAsync($"/api/clients/individual/{nonExistentId}", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_Returns204_WhenAdmin()
    {
        // Arrange
        var client = CreateAuthenticatedClient(AdminUserId, "Admin");
        var createDto = new CreateIndividualClientDto
        {
            FirstName = "Carol",
            LastName = "White",
            EGN = "5555555555",
            Email = "carol.white.deactivate@example.com"
        };
        var createResponse = await client.PostAsync("/api/clients/individual", JsonBody(createDto));
        var body = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<IndividualClientResponseDto>(body, JsonOptions);

        // Act
        var response = await client.PatchAsync($"/api/clients/{created!.Id}/deactivate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Deactivate_Returns403_WhenEmployee()
    {
        // Arrange — auth check happens before service; no real client needed
        var client = CreateAuthenticatedClient(Guid.NewGuid(), "Employee");

        // Act
        var response = await client.PatchAsync($"/api/clients/{Guid.NewGuid()}/deactivate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
