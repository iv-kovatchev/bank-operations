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
        Type = "Individual",
        FirstName = client.FirstName,
        LastName = client.LastName,
        Egn = client.EGN
    };

    public static CorporateClientResponseDto ToDto(CorporateClient client) => new()
    {
        Id = client.ClientId,
        Email = client.User.Email!,
        IsActive = client.User.IsActive,
        CreatedAt = client.User.CreatedAt,
        CreatedByUserId = client.CreatedByUserId,
        Type = "Corporate",
        CompanyName = client.CompanyName,
        Eik = client.EIK,
        RepresentativeFirstName = client.RepresentativeFirstName,
        RepresentativeLastName = client.RepresentativeLastName
    };

    public static ClientResponseDto ToDto(Client client)
    {
        var dto = new ClientResponseDto
        {
            Id = client.ClientId,
            Email = client.User.Email!,
            IsActive = client.User.IsActive,
            CreatedAt = client.User.CreatedAt,
            CreatedByUserId = client.CreatedByUserId
        };

        if (client is IndividualClient ic)
        {
            dto.Type = "Individual";
            dto.FirstName = ic.FirstName;
            dto.LastName = ic.LastName;
            dto.Egn = ic.EGN;
        }
        else if (client is CorporateClient cc)
        {
            dto.Type = "Corporate";
            dto.CompanyName = cc.CompanyName;
            dto.Eik = cc.EIK;
            dto.RepresentativeFirstName = cc.RepresentativeFirstName;
            dto.RepresentativeLastName = cc.RepresentativeLastName;
        }

        return dto;
    }
}
