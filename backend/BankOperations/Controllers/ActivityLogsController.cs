using BankOperations.Services.ActivityLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOperations.Controllers;

[ApiController]
[Route("api/activity-logs")]
[Authorize(Roles = "Admin")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _activityLogService.GetAllLogsAsync();
        return Ok(result);
    }
}
