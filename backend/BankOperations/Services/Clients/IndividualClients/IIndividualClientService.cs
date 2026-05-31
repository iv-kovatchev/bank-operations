using BankOperations.DTOs.Clients.IndividualClients;

namespace BankOperations.Services.Clients.IndividualClients;

public interface IIndividualClientService
{
    Task<IndividualClientResponseDto> CreateAsync(CreateIndividualClientDto dto, Guid createdByUserId);
    Task<IndividualClientResponseDto> UpdateAsync(Guid id, UpdateIndividualClientDto dto);
}
