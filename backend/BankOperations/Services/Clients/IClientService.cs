using BankOperations.DTOs.Clients;

namespace BankOperations.Services.Clients;

public interface IClientService
{
    Task<ClientResponseDto> GetClientByIdAsync(Guid id);
    Task<IEnumerable<ClientResponseDto>> GetAllClientsAsync();
    Task DeactivateClientAsync(Guid id);
}
