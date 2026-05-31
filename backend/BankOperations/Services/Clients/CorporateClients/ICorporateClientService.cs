using BankOperations.DTOs.Clients.CorporateClients;

namespace BankOperations.Services.Clients.CorporateClients;

public interface ICorporateClientService
{
    Task<CorporateClientResponseDto> CreateAsync(CreateCorporateClientDto dto, Guid createdByUserId);
    Task<CorporateClientResponseDto> UpdateAsync(Guid id, UpdateCorporateClientDto dto, Guid requestingUserId, bool isAdmin);
}
