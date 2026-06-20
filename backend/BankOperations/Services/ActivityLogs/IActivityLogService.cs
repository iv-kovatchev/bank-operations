using BankOperations.DTOs.ActivityLogs;

namespace BankOperations.Services.ActivityLogs;

public interface IActivityLogService
{
    Task LogAsync(Guid userId, string action, string entityType, Guid entityId, string? details = null);
    Task<IEnumerable<ActivityLogResponseDto>> GetAllLogsAsync();
}
