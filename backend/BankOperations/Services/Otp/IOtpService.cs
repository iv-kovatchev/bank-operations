namespace BankOperations.Services.Otp;

public interface IOtpService
{
    Task<string> GenerateAndSaveOtpAsync(Guid userId);
    Task<bool> ValidateOtpAsync(Guid userId, string code);
}
