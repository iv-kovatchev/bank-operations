using System.Security.Claims;
using BankOperations.Services.Stats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOperations.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize(Roles = "Employee,Admin")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _statsService.GetStatsAsync(userId, isAdmin);
        return Ok(result);
    }
}
