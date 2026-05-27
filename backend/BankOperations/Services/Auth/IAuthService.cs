using BankOperations.DTOs.Auth;

namespace BankOperations.Services.Auth;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<AuthResultDto> VerifyOtpAsync(VerifyOtpDto dto);
    Task<AuthResultDto> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}
