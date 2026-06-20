using System.Security.Claims;
using BankOperations.DTOs.Settings;
using BankOperations.Services.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOperations.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _settingsService.GetProfileAsync(userId);
        return Ok(result);
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _settingsService.UpdateProfileAsync(userId, dto);
        return Ok(result);
    }

    [HttpPatch("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _settingsService.ChangePasswordAsync(userId, dto);
        return NoContent();
    }
}
