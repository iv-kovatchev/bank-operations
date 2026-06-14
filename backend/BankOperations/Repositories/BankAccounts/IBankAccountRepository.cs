using BankOperations.Entities;

namespace BankOperations.Repositories.BankAccounts;

public interface IBankAccountRepository : IRepository<BankAccount>
{
    Task<IEnumerable<BankAccount>> GetAllByClientIdAsync(Guid clientId);
    Task<bool> ExistsByIbanAsync(string iban);
    Task<BankAccount?> GetByIdWithClientAsync(Guid id);
}
