using BankOperations.Entities.Clients;

namespace BankOperations.Repositories.Clients;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Client>> GetAllWithDetailsAsync();
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByEGNAsync(string egn);
    Task<bool> ExistsByEIKAsync(string eik);
}
