using BankOperations.DTOs.ActivityLogs;
using BankOperations.Entities;

namespace BankOperations.Mappers.ActivityLogs;

public static class ActivityLogMapper
{
    public static ActivityLogResponseDto ToDto(ActivityLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        UserName = $"{log.User.FirstName} {log.User.LastName}",
        Action = log.Action,
        EntityType = log.EntityType,
        EntityId = log.EntityId,
        Timestamp = log.Timestamp,
        Details = log.Details
    };
}
