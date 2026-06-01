using BankOperations.DTOs.Clients;

namespace BankOperations.Services.Clients;

public interface IClientService
{
    Task<ClientResponseDto> GetClientByIdAsync(Guid id, Guid requestingUserId, bool isAdmin);
    Task<IEnumerable<ClientResponseDto>> GetAllClientsAsync(Guid? createdByUserId = null);
    Task DeactivateClientAsync(Guid id);
    Task ActivateClientAsync(Guid id);
}
