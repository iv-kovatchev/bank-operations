using BankOperations.DTOs.Clients;
using BankOperations.Exceptions;
using BankOperations.Mappers.Clients;
using BankOperations.Repositories.Clients;

namespace BankOperations.Services.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientResponseDto> GetClientByIdAsync(Guid id, Guid requestingUserId, bool isAdmin)
    {
        var client = await _clientRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException("Client", id);

        if (!isAdmin && client.CreatedByUserId != requestingUserId)
            throw new UnauthorizedException("You do not have access to this client.");

        return ClientMapper.ToDto(client);
    }

    public async Task<IEnumerable<ClientResponseDto>> GetAllClientsAsync(Guid? createdByUserId = null)
    {
        var clients = await _clientRepository.GetAllWithDetailsAsync(createdByUserId);
        return clients.Select(ClientMapper.ToDto);
    }

    public async Task DeactivateClientAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException("Client", id);

        client.User.IsActive = false;
        await _clientRepository.UpdateAsync(client);
        await _clientRepository.SaveChangesAsync();
    }
}
