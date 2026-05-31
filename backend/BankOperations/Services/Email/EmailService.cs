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
        var (smtpClient, from, _) = CreateSmtpClient();

        var subject = "Your login verification code";
        var body = $"Hello {firstName}, your verification code is: {otpCode}. It expires in 5 minutes.";

        using var message = new MailMessage
        {
            From = from,
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using (smtpClient)
            await smtpClient.SendMailAsync(message);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName, string password)
    {
        var (smtpClient, from, _) = CreateSmtpClient();

        var subject = "Welcome to Bank Operations";
        var body = $"Hello {firstName}, your account has been created.\nEmail: {toEmail}\nTemporary password: {password}\nPlease change your password after your first login.";

        using var message = new MailMessage
        {
            From = from,
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using (smtpClient)
            await smtpClient.SendMailAsync(message);
    }

    private (SmtpClient client, MailAddress from, string fromName) CreateSmtpClient()
    {
        var host = _configuration["Email:SmtpHost"]!;
        var port = int.Parse(_configuration["Email:SmtpPort"]!);

        var fromEmail = _configuration["EMAIL_ADDRESS"]
            ?? _configuration["Email:FromEmail"]
            ?? throw new InvalidOperationException("EMAIL_ADDRESS is not configured.");

        var fromName = _configuration["Email:FromName"]!;

        var password = _configuration["EMAIL_PASSWORD"]
            ?? _configuration["Email:Password"]
            ?? throw new InvalidOperationException("EMAIL_PASSWORD is not configured.");

        var smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true
        };

        return (smtpClient, new MailAddress(fromEmail, fromName), fromName);
    }
}
