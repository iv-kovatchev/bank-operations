using BankOperations.DTOs.Settings;

namespace BankOperations.Services.Settings;

public interface ISettingsService
{
    Task<ProfileResponseDto> GetProfileAsync(Guid userId);
    Task<ProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
}
