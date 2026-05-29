using System.Net;
using System.Net.Mail;

namespace BankOperations.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOtpEmailAsync(string toEmail, string firstName, string otpCode)
    {
        var host = _configuration["Email:SmtpHost"]!;
        var port = int.Parse(_configuration["Email:SmtpPort"]!);

        var fromEmail = _configuration["EMAIL_ADDRESS"]
            ?? Environment.GetEnvironmentVariable("EMAIL_ADDRESS")
            ?? throw new InvalidOperationException("EMAIL_ADDRESS is not configured.");

        var fromName = _configuration["Email:FromName"]!;

        var password = _configuration["EMAIL_PASSWORD"]
            ?? Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
            ?? throw new InvalidOperationException("EMAIL_PASSWORD is not configured.");

        var subject = "Your login verification code";
        var body = $"Hello {firstName}, your verification code is: {otpCode}. It expires in 5 minutes.";

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }
}
