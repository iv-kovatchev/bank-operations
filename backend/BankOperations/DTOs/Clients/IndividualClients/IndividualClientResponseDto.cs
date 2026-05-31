using BankOperations.DTOs.Clients;

namespace BankOperations.DTOs.Clients.IndividualClients;

public class IndividualClientResponseDto : ClientResponseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EGN { get; set; } = string.Empty;
}
