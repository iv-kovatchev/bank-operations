using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BankOperations.DTOs.BankAccounts;
using BankOperations.DTOs.Clients.IndividualClients;
using BankOperations.Tests.Integration.Clients;
using Shouldly;

namespace BankOperations.Tests.Integration.BankAccounts;

[Collection("IntegrationTests")]
public class BankAccountsIntegrationTests : IClassFixture<BankOperationsWebApplicationFactory>
{
    private readonly BankOperationsWebApplicationFactory _factory;
    private static readonly Guid EmployeeUserId = Guid.NewGuid();
    private static readonly Guid AdminUserId = Guid.NewGuid();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public BankAccountsIntegrationTests(BankOperationsWebApplicationFactory factory)
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

    private async Task<Guid> CreateClientAsync(HttpClient http, string egn, string email)
    {
        var dto = new CreateIndividualClientDto
        {
            FirstName = "Test",
            LastName = "Client",
            EGN = egn,
            Email = email
        };
        var response = await http.PostAsync("/api/clients/individual", JsonBody(dto));
        var body = await response.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<IndividualClientResponseDto>(body, JsonOptions);
        return created!.Id;
    }

    [Fact]
    public async Task GetAccounts_Returns401_WhenUnauthenticated()
    {
        // Arrange
        var client = CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/clients/{Guid.NewGuid()}/accounts");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAccounts_Returns200_WhenEmployee()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "6666666666", "bankacct.test1@example.com");

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.GetAsync($"/api/clients/{clientId}/accounts");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenAccount_Returns201_WhenEmployee()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "7777777777", "bankacct.test2@example.com");

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var dto = new CreateBankAccountDto { IBAN = "BG99BANK00000000000001", InitialBalance = 1000 };

        // Act
        var response = await employeeClient.PostAsync($"/api/clients/{clientId}/accounts", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task OpenAccount_Returns409_WhenIbanAlreadyExists()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "8888888888", "bankacct.test3@example.com");

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var dto = new CreateBankAccountDto { IBAN = "BG99BANK00000000000002", InitialBalance = 500 };

        // Act — first open succeeds, second with same IBAN conflicts
        await employeeClient.PostAsync($"/api/clients/{clientId}/accounts", JsonBody(dto));
        var response = await employeeClient.PostAsync($"/api/clients/{clientId}/accounts", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CloseAccount_Returns204_WhenAdmin()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "9999999999", "bankacct.test4@example.com");

        var openDto = new CreateBankAccountDto { IBAN = "BG99BANK00000000000003", InitialBalance = 0 };
        var openResponse = await adminClient.PostAsync($"/api/clients/{clientId}/accounts", JsonBody(openDto));
        var openBody = await openResponse.Content.ReadAsStringAsync();
        var account = JsonSerializer.Deserialize<BankAccountResponseDto>(openBody, JsonOptions);

        // Act
        var response = await adminClient.PatchAsync($"/api/accounts/{account!.Id}/close", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CloseAccount_Returns404_WhenEmployee_AccountNotFound()
    {
        // Arrange — employees are now authorized; a random account ID yields 404 from service
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.PatchAsync($"/api/accounts/{Guid.NewGuid()}/close", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAccount_Returns204_WhenAdmin()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "1010101010", "bankacct.delete1@example.com");

        var openDto = new CreateBankAccountDto { IBAN = "BG99BANK00000000000004", InitialBalance = 0 };
        var openResponse = await adminClient.PostAsync($"/api/clients/{clientId}/accounts", JsonBody(openDto));
        var account = JsonSerializer.Deserialize<BankAccountResponseDto>(
            await openResponse.Content.ReadAsStringAsync(), JsonOptions);

        // Act
        var response = await adminClient.DeleteAsync($"/api/accounts/{account!.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAccount_Returns403_WhenEmployee()
    {
        // Arrange — auth check runs before service; no real account needed
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.DeleteAsync($"/api/accounts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
