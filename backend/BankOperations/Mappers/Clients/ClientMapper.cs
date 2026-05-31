using BankOperations.DTOs.Clients;
using BankOperations.DTOs.Clients.CorporateClients;
using BankOperations.DTOs.Clients.IndividualClients;
using BankOperations.Entities.Clients;

namespace BankOperations.Mappers.Clients;

public static class ClientMapper
{
    public static IndividualClientResponseDto ToDto(IndividualClient client) => new()
    {
        Id = client.ClientId,
        Email = client.User.Email!,
        IsActive = client.User.IsActive,
        CreatedAt = client.User.CreatedAt,
        CreatedByUserId = client.CreatedByUserId,
        FirstName = client.FirstName,
        LastName = client.LastName,
        EGN = client.EGN
    };

    public static CorporateClientResponseDto ToDto(CorporateClient client) => new()
    {
        Id = client.ClientId,
        Email = client.User.Email!,
        IsActive = client.User.IsActive,
        CreatedAt = client.User.CreatedAt,
        CreatedByUserId = client.CreatedByUserId,
        CompanyName = client.CompanyName,
        EIK = client.EIK,
        RepresentativeFirstName = client.RepresentativeFirstName,
        RepresentativeLastName = client.RepresentativeLastName
    };

    public static ClientResponseDto ToDto(Client client) => client switch
    {
        IndividualClient ic => ToDto(ic),
        CorporateClient cc => ToDto(cc),
        _ => throw new InvalidOperationException($"Unknown client type: {client.GetType().Name}")
    };
}
