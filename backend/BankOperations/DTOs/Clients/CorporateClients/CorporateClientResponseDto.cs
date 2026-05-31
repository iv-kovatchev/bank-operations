using BankOperations.DTOs.Clients;

namespace BankOperations.DTOs.Clients.CorporateClients;

public class CorporateClientResponseDto : ClientResponseDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string EIK { get; set; } = string.Empty;
    public string RepresentativeFirstName { get; set; } = string.Empty;
    public string RepresentativeLastName { get; set; } = string.Empty;
}
