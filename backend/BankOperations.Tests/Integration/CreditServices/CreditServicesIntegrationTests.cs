using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BankOperations.DTOs.CreditServices;
using BankOperations.Enums;
using BankOperations.Tests.Integration.Clients;
using Shouldly;

namespace BankOperations.Tests.Integration.CreditServices;

[Collection("IntegrationTests")]
public class CreditServicesIntegrationTests : IClassFixture<BankOperationsWebApplicationFactory>
{
    private readonly BankOperationsWebApplicationFactory _factory;
    private static readonly Guid EmployeeUserId = Guid.NewGuid();
    private static readonly Guid AdminUserId = Guid.NewGuid();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CreditServicesIntegrationTests(BankOperationsWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateUnauthenticatedClient() =>
        _factory.CreateClient(new() { AllowAutoRedirect = false });

    private HttpClient CreateAuthenticatedClient(Guid userId, string role)
    {
        var token = _factory.GenerateJwtToken(userId, role);
        var client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static StringContent JsonBody(object dto) =>
        new(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

    private static CreateCreditServiceDto CreateDto(string name) => new()
    {
        Name = name,
        Type = CreditType.Consumer,
        InterestRate = 5.5m,
        MaxAmount = 10000,
        MaxTermMonths = 36
    };

    private async Task<Guid> CreateCreditServiceAsync(HttpClient http, string name)
    {
        var response = await http.PostAsync("/api/creditservices", JsonBody(CreateDto(name)));
        var body = await response.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<CreditServiceResponseDto>(body, JsonOptions);
        return created!.Id;
    }

    [Fact]
    public async Task GetAll_Returns200_WhenEmployee()
    {
        // Arrange
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.GetAsync("/api/creditservices");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_Returns401_WhenUnauthenticated()
    {
        // Arrange
        var client = CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/creditservices");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_Returns200_WhenFound()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var id = await CreateCreditServiceAsync(adminClient, "Consumer GetById Test");

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.GetAsync($"/api/creditservices/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotFound()
    {
        // Arrange
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.GetAsync($"/api/creditservices/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Returns201_WhenValid()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var dto = CreateDto("Consumer Create Test");

        // Act
        var response = await adminClient.PostAsync("/api/creditservices", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_Returns409_WhenNameExists()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var dto = CreateDto("Consumer Conflict Test");
        await adminClient.PostAsync("/api/creditservices", JsonBody(dto));

        // Act
        var response = await adminClient.PostAsync("/api/creditservices", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_Returns403_WhenEmployee()
    {
        // Arrange
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var dto = CreateDto("Consumer Forbidden Test");

        // Act
        var response = await employeeClient.PostAsync("/api/creditservices", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_Returns200_WhenValid()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var id = await CreateCreditServiceAsync(adminClient, "Consumer Update Test");
        var dto = new UpdateCreditServiceDto
        {
            Name = "Consumer Update Test Updated",
            Type = CreditType.Consumer,
            InterestRate = 6.0m,
            MaxAmount = 15000,
            MaxTermMonths = 48
        };

        // Act
        var response = await adminClient.PutAsync($"/api/creditservices/{id}", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_Returns404_WhenNotFound()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var dto = new UpdateCreditServiceDto
        {
            Name = "Consumer Update Missing Test",
            Type = CreditType.Consumer,
            InterestRate = 6.0m,
            MaxAmount = 15000,
            MaxTermMonths = 48
        };

        // Act
        var response = await adminClient.PutAsync($"/api/creditservices/{Guid.NewGuid()}", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Returns204_WhenValid()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var id = await CreateCreditServiceAsync(adminClient, "Consumer Delete Test");

        // Act
        var response = await adminClient.DeleteAsync($"/api/creditservices/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Returns403_WhenEmployee()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var id = await CreateCreditServiceAsync(adminClient, "Consumer Delete Forbidden Test");

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.DeleteAsync($"/api/creditservices/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
