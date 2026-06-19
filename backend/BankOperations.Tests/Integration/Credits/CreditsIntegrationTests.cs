using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BankOperations.DTOs.Clients.IndividualClients;
using BankOperations.DTOs.Credits;
using BankOperations.DTOs.Credits.ConsumerCredits;
using BankOperations.DTOs.Credits.MortgageCredits;
using BankOperations.DTOs.CreditServices;
using BankOperations.Enums;
using BankOperations.Tests.Integration.Clients;
using Shouldly;

namespace BankOperations.Tests.Integration.Credits;

[Collection("IntegrationTests")]
public class CreditsIntegrationTests : IClassFixture<BankOperationsWebApplicationFactory>
{
    private readonly BankOperationsWebApplicationFactory _factory;
    private static readonly Guid EmployeeUserId = Guid.NewGuid();
    private static readonly Guid AdminUserId = Guid.NewGuid();
    private static readonly Guid ClientUserId = Guid.NewGuid();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CreditsIntegrationTests(BankOperationsWebApplicationFactory factory)
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

    private async Task<Guid> CreateCreditServiceAsync(HttpClient adminHttp, string name, decimal maxAmount, int maxTermMonths, CreditType type = CreditType.Consumer)
    {
        var dto = new CreateCreditServiceDto
        {
            Name = name,
            Type = type,
            InterestRate = 5.5m,
            MaxAmount = maxAmount,
            MaxTermMonths = maxTermMonths
        };
        var response = await adminHttp.PostAsync("/api/creditservices", JsonBody(dto));
        var body = await response.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<CreditServiceResponseDto>(body, JsonOptions);
        return created!.Id;
    }

    private static async Task<CreditResponseDto> ReadCreditAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreditResponseDto>(body, JsonOptions)!;
    }

    [Fact]
    public async Task GetAll_Returns401_WhenUnauthenticated()
    {
        // Arrange
        var client = CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/clients/{Guid.NewGuid()}/credits");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_Returns200_WhenEmployee()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "4000000001", "credits.test1@example.com");

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.GetAsync($"/api/clients/{clientId}/credits");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_Returns403_WhenClientAccessesAnotherClientsCredits()
    {
        // Arrange
        var clientRoleClient = CreateAuthenticatedClient(ClientUserId, "Client");

        // Act
        var response = await clientRoleClient.GetAsync($"/api/clients/{Guid.NewGuid()}/credits");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GrantConsumerCredit_Returns201_WhenEmployee()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "4000000002", "credits.test2@example.com");
        var creditServiceId = await CreateCreditServiceAsync(adminClient, "Credits Module - Consumer Grant Test", maxAmount: 10000, maxTermMonths: 36);

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var dto = new CreateConsumerCreditDto
        {
            CreditServiceId = creditServiceId,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };

        // Act
        var response = await employeeClient.PostAsync($"/api/clients/{clientId}/credits/consumer", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GrantConsumerCredit_Returns400_WhenAmountExceedsMax()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "4000000003", "credits.test3@example.com");
        var creditServiceId = await CreateCreditServiceAsync(adminClient, "Credits Module - Consumer Limit Test", maxAmount: 500, maxTermMonths: 36);

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var dto = new CreateConsumerCreditDto
        {
            CreditServiceId = creditServiceId,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };

        // Act
        var response = await employeeClient.PostAsync($"/api/clients/{clientId}/credits/consumer", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GrantMortgageCredit_Returns201_WhenEmployee()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var clientId = await CreateClientAsync(adminClient, "4000000004", "credits.test4@example.com");
        var creditServiceId = await CreateCreditServiceAsync(adminClient, "Credits Module - Mortgage Grant Test", maxAmount: 200000, maxTermMonths: 360, type: CreditType.Mortgage);

        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var dto = new CreateMortgageCreditDto
        {
            CreditServiceId = creditServiceId,
            Amount = 100000,
            TermMonths = 240,
            PropertyAddress = "1 Main St",
            PropertyType = PropertyType.Apartment
        };

        // Act
        var response = await employeeClient.PostAsync($"/api/clients/{clientId}/credits/mortgage", JsonBody(dto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetById_Returns200_WhenEmployee()
    {
        // Arrange — the requesting employee must also be the one who created the client (ownership is anchored on Client.CreatedByUserId)
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var clientId = await CreateClientAsync(employeeClient, "4000000005", "credits.test5@example.com");

        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var creditServiceId = await CreateCreditServiceAsync(adminClient, "Credits Module - Consumer GetById Test", maxAmount: 10000, maxTermMonths: 36);

        var dto = new CreateConsumerCreditDto
        {
            CreditServiceId = creditServiceId,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        var grantResponse = await employeeClient.PostAsync($"/api/clients/{clientId}/credits/consumer", JsonBody(dto));
        var credit = await ReadCreditAsync(grantResponse);

        // Act
        var response = await employeeClient.GetAsync($"/api/credits/{credit.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotFound()
    {
        // Arrange
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");

        // Act
        var response = await employeeClient.GetAsync($"/api/credits/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRepaymentPlan_Returns200_WhenEmployee()
    {
        // Arrange — same ownership requirement as GetById
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var clientId = await CreateClientAsync(employeeClient, "4000000006", "credits.test6@example.com");

        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var creditServiceId = await CreateCreditServiceAsync(adminClient, "Credits Module - Consumer RepaymentPlan Test", maxAmount: 10000, maxTermMonths: 36);

        var dto = new CreateConsumerCreditDto
        {
            CreditServiceId = creditServiceId,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        var grantResponse = await employeeClient.PostAsync($"/api/clients/{clientId}/credits/consumer", JsonBody(dto));
        var credit = await ReadCreditAsync(grantResponse);

        // Act
        var response = await employeeClient.GetAsync($"/api/credits/{credit.Id}/repayment-plan");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateConsumerCredit_Returns200_WhenNoInstallmentsPaid()
    {
        // Arrange
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var clientId = await CreateClientAsync(employeeClient, "4000000007", "credits.test7@example.com");

        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var creditServiceId = await CreateCreditServiceAsync(adminClient, "Credits Module - Consumer Update Test", maxAmount: 10000, maxTermMonths: 36);

        var grantDto = new CreateConsumerCreditDto
        {
            CreditServiceId = creditServiceId,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        var grantResponse = await employeeClient.PostAsync($"/api/clients/{clientId}/credits/consumer", JsonBody(grantDto));
        var credit = await ReadCreditAsync(grantResponse);

        var updateDto = new UpdateConsumerCreditDto
        {
            CreditServiceId = creditServiceId,
            Amount = 2000,
            TermMonths = 24,
            Purpose = CreditPurpose.Education
        };

        // Act
        var response = await employeeClient.PutAsync($"/api/credits/{credit.Id}/consumer", JsonBody(updateDto));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateConsumerCredit_Returns404_WhenCreditServiceNotFound()
    {
        // Arrange
        var employeeClient = CreateAuthenticatedClient(EmployeeUserId, "Employee");
        var clientId = await CreateClientAsync(employeeClient, "4000000008", "credits.test8@example.com");

        var adminClient = CreateAuthenticatedClient(AdminUserId, "Admin");
        var creditServiceId = await CreateCreditServiceAsync(adminClient, "Credits Module - Consumer Update Missing Test", maxAmount: 10000, maxTermMonths: 36);

        var grantDto = new CreateConsumerCreditDto
        {
            CreditServiceId = creditServiceId,
            Amount = 1000,
            TermMonths = 12,
            Purpose = CreditPurpose.Other
        };
        var grantResponse = await employeeClient.PostAsync($"/api/clients/{clientId}/credits/consumer", JsonBody(grantDto));
        var credit = await ReadCreditAsync(grantResponse);

        // dto.CreditServiceId differs from the credit's current CreditServiceId and does not exist
        var updateDto = new UpdateConsumerCreditDto
        {
            CreditServiceId = Guid.NewGuid(),
            Amount = 2000,
            TermMonths = 24,
            Purpose = CreditPurpose.Education
        };

        // Act
        var response = await employeeClient.PutAsync($"/api/credits/{credit.Id}/consumer", JsonBody(updateDto));

        // Assert — service throws NotFoundException("CreditService", ...), which the global middleware maps to 404
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
