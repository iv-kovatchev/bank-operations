namespace BankOperations.Repositories.Stats;

public interface IStatsRepository
{
    Task<int> GetTotalClientsAsync(Guid? createdByUserId);
    Task<int> GetActiveClientsAsync(Guid? createdByUserId);
    Task<int> GetTotalBankAccountsAsync(Guid? createdByUserId);
    Task<int> GetActiveBankAccountsAsync(Guid? createdByUserId);
    Task<int> GetTotalCreditsAsync(Guid? createdByUserId);
    Task<int> GetActiveCreditsAsync(Guid? createdByUserId);
    Task<decimal> GetTotalCreditAmountAsync(Guid? createdByUserId);
    Task<decimal> GetTotalBalanceAsync(Guid? createdByUserId);
}
