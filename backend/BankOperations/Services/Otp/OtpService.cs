using System.Security.Cryptography;
using BankOperations.Entities;
using BankOperations.Repositories.Otp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace BankOperations.Services.Otp;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<ApplicationUser> _userManager;

    public OtpService(
        IOtpRepository otpRepository,
        IWebHostEnvironment environment,
        UserManager<ApplicationUser> userManager)
    {
        _otpRepository = otpRepository;
        _environment = environment;
        _userManager = userManager;
    }

    public async Task<string> GenerateAndSaveOtpAsync(Guid userId)
    {
        if (_environment.IsDevelopment())
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null && await _userManager.IsInRoleAsync(user, "Employee"))
            {
                await _otpRepository.InvalidateAllForUserAsync(userId);
                var devOtp = new OtpCode
                {
                    UserId = userId,
                    Code = "000000",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };
                await _otpRepository.SaveAsync(devOtp);
                return "000000";
            }
        }

        await _otpRepository.InvalidateAllForUserAsync(userId);

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var otp = new OtpCode
        {
            UserId = userId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _otpRepository.SaveAsync(otp);

        return code;
    }

    public async Task<bool> ValidateOtpAsync(Guid userId, string code)
    {
        var otp = await _otpRepository.GetValidAsync(userId, code);

        if (otp is null)
            return false;

        await _otpRepository.MarkAsUsedAsync(otp.Id);
        return true;
    }
}
