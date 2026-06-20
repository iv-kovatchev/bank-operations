using BankOperations.DTOs.ActivityLogs;
using BankOperations.Entities;
using BankOperations.Mappers.ActivityLogs;
using BankOperations.Repositories.ActivityLogs;

namespace BankOperations.Services.ActivityLogs;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(IActivityLogRepository activityLogRepository, ILogger<ActivityLogService> logger)
    {
        _activityLogRepository = activityLogRepository;
        _logger = logger;
    }

    public async Task LogAsync(Guid userId, string action, string entityType, Guid entityId, string? details = null)
    {
        try
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details
            };

            await _activityLogRepository.AddAsync(log);
            await _activityLogRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write activity log for action {Action} on {EntityType} {EntityId}", action, entityType, entityId);
        }
    }

    public async Task<IEnumerable<ActivityLogResponseDto>> GetAllLogsAsync()
    {
        var logs = await _activityLogRepository.GetAllAsync();
        return logs.Select(ActivityLogMapper.ToDto);
    }
}
