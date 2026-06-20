using BankOperations.Entities;
using BankOperations.Entities.Credits;

namespace BankOperations.Repositories.Credits;

public interface ICreditRepository : IRepository<Credit>
{
    Task<IEnumerable<Credit>> GetAllByClientIdAsync(Guid clientId);
    Task<Credit?> GetByIdWithDetailsAsync(Guid id);
    Task<RepaymentPlan?> GetRepaymentPlanAsync(Guid creditId);
    Task AddRepaymentPlanAsync(RepaymentPlan plan);
    Task DeleteRepaymentPlanByCreditIdAsync(Guid creditId);
    Task<RepaymentInstallment?> GetInstallmentByIdAsync(Guid installmentId);
}
