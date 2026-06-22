using BankOperations.DTOs.Stats;

namespace BankOperations.Services.Stats;

public interface IStatsService
{
    Task<StatsResponseDto> GetStatsAsync(Guid requestingUserId, bool isAdmin);
}
