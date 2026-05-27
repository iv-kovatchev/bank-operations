using BankOperations.DTOs.Auth;
using BankOperations.Entities;
using BankOperations.Exceptions;
using BankOperations.Services.Email;
using BankOperations.Services.Otp;
using BankOperations.Services.Token;
using Microsoft.AspNetCore.Identity;

namespace BankOperations.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IOtpService otpService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new NotFoundException("User", dto.Email);

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            throw new UnauthorizedException("Invalid credentials.");

        var otpCode = await _otpService.GenerateAndSaveOtpAsync(user.Id);
        await _emailService.SendOtpEmailAsync(user.Email!, user.FirstName, otpCode);

        return new AuthResultDto
        {
            RequiresOtp = true,
            Message = "OTP sent to your email."
        };
    }

    public async Task<AuthResultDto> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new NotFoundException("User", dto.Email);

        var otpValid = await _otpService.ValidateOtpAsync(user.Id, dto.Code);
        if (!otpValid)
            throw new UnauthorizedException("Invalid or expired OTP code.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RequiresOtp = false,
            Message = "Login successful."
        };
    }

    public async Task<AuthResultDto> RefreshAsync(string refreshToken)
    {
        var storedToken = await _tokenService.GetRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString())
            ?? throw new NotFoundException("User", storedToken.UserId);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RequiresOtp = false,
            Message = "Token refreshed."
        };
    }

    public async Task LogoutAsync(string refreshToken)
        => await _tokenService.RevokeRefreshTokenAsync(refreshToken);
}
