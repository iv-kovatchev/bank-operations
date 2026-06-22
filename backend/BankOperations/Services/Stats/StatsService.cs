using BankOperations.DTOs.Stats;
using BankOperations.Repositories.Stats;

namespace BankOperations.Services.Stats;

public class StatsService : IStatsService
{
    private readonly IStatsRepository _statsRepository;

    public StatsService(IStatsRepository statsRepository)
    {
        _statsRepository = statsRepository;
    }

    public async Task<StatsResponseDto> GetStatsAsync(Guid requestingUserId, bool isAdmin)
    {
        Guid? createdByUserId = isAdmin ? null : requestingUserId;

        return new StatsResponseDto
        {
            TotalClients = await _statsRepository.GetTotalClientsAsync(createdByUserId),
            ActiveClients = await _statsRepository.GetActiveClientsAsync(createdByUserId),
            TotalBankAccounts = await _statsRepository.GetTotalBankAccountsAsync(createdByUserId),
            ActiveBankAccounts = await _statsRepository.GetActiveBankAccountsAsync(createdByUserId),
            TotalCredits = await _statsRepository.GetTotalCreditsAsync(createdByUserId),
            ActiveCredits = await _statsRepository.GetActiveCreditsAsync(createdByUserId),
            TotalCreditAmount = await _statsRepository.GetTotalCreditAmountAsync(createdByUserId),
            TotalBalance = await _statsRepository.GetTotalBalanceAsync(createdByUserId)
        };
    }
}
