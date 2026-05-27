using System.Security.Cryptography;
using BankOperations.Entities;
using BankOperations.Repositories.Otp;

namespace BankOperations.Services.Otp;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;

    public OtpService(IOtpRepository otpRepository)
    {
        _otpRepository = otpRepository;
    }

    public async Task<string> GenerateAndSaveOtpAsync(Guid userId)
    {
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
