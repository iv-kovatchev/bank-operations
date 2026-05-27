namespace BankOperations.Services.Email;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string firstName, string otpCode);
}
