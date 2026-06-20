using BankOperations.DTOs.Settings;
using BankOperations.Entities;
using BankOperations.Exceptions;
using BankOperations.Services.ActivityLogs;
using Microsoft.AspNetCore.Identity;

namespace BankOperations.Services.Settings;

public class SettingsService : ISettingsService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public SettingsService(
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLogService)
    {
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    public async Task<ProfileResponseDto> GetProfileAsync(Guid userId)
    {
        var user = await GetUserAsync(userId);
        return await ToDtoAsync(user);
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await GetUserAsync(userId);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        await _userManager.UpdateAsync(user);

        await _activityLogService.LogAsync(userId, "UpdateProfile", "User", userId, $"Updated profile for {user.Email}");

        return await ToDtoAsync(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await GetUserAsync(userId);

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _activityLogService.LogAsync(userId, "ChangePassword", "User", userId, $"Changed password for {user.Email}");
    }

    private async Task<ApplicationUser> GetUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException("User", userId);

        return user;
    }

    private async Task<ProfileResponseDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new ProfileResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty
        };
    }
}
