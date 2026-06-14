using BankOperations.Entities;

namespace BankOperations.Repositories.CreditServices;

public interface ICreditServiceRepository : IRepository<CreditService>
{
    Task<bool> ExistsByNameAsync(string name);
}
