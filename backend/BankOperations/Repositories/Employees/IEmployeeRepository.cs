using BankOperations.Entities;

namespace BankOperations.Repositories.Employees;

public interface IEmployeeRepository : IRepository<ApplicationUser>
{
    Task<bool> ExistsByEmailAsync(string email);
}
