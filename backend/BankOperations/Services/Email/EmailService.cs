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

        var contentHtml = $"""
            <p style="margin:0 0 16px;font-size:15px;color:#1b2a1e;">Hello {WebUtility.HtmlEncode(firstName)},</p>
            <p style="margin:0 0 20px;font-size:14px;color:#3a3a3a;line-height:1.5;">Use the verification code below to finish signing in.</p>
            <div style="margin:0 0 20px;padding:16px;background-color:#f1f1f1;border:1px solid #d0d0d0;border-radius:6px;text-align:center;">
                <span style="font-family:'Courier New',Courier,monospace;font-size:28px;font-weight:bold;letter-spacing:4px;color:#1b2a1e;">{WebUtility.HtmlEncode(otpCode)}</span>
            </div>
            <p style="margin:0;font-size:13px;color:#777777;">This code expires in 5 minutes.</p>
            """;

        using var message = new MailMessage
        {
            From = from,
            Subject = subject,
            Body = BuildHtmlEmail("Login Verification Code", contentHtml),
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        using (smtpClient)
            await smtpClient.SendMailAsync(message);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName, string password)
    {
        var (smtpClient, from, _) = CreateSmtpClient();

        var subject = "Welcome to Bank Operations";

        var contentHtml = $"""
            <p style="margin:0 0 16px;font-size:15px;color:#1b2a1e;">Hello {WebUtility.HtmlEncode(firstName)},</p>
            <p style="margin:0 0 20px;font-size:14px;color:#3a3a3a;line-height:1.5;">Your account has been created. Use the credentials below to log in.</p>
            <div style="margin:0 0 20px;padding:16px;background-color:#f1f1f1;border:1px solid #d0d0d0;border-radius:6px;">
                <p style="margin:0 0 10px;font-size:14px;color:#1b2a1e;">Email: <strong>{WebUtility.HtmlEncode(toEmail)}</strong></p>
                <p style="margin:0;font-size:14px;color:#1b2a1e;">Temporary password: <span style="font-family:'Courier New',Courier,monospace;font-weight:bold;">{WebUtility.HtmlEncode(password)}</span></p>
            </div>
            <p style="margin:0;font-size:13px;color:#777777;">Please change your password after your first login.</p>
            """;

        using var message = new MailMessage
        {
            From = from,
            Subject = subject,
            Body = BuildHtmlEmail("Welcome to Bank Operations", contentHtml),
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        using (smtpClient)
            await smtpClient.SendMailAsync(message);
    }

    private static string BuildHtmlEmail(string title, string contentHtml)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <body style="margin:0;padding:24px 0;background-color:#f0f0f0;font-family:Arial,Helvetica,sans-serif;">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#f0f0f0;">
                    <tr>
                        <td align="center">
                            <table role="presentation" width="480" cellpadding="0" cellspacing="0" style="max-width:480px;width:100%;">
                                <tr>
                                    <td style="background-color:#386644;border-radius:8px 8px 0 0;padding:20px 24px;">
                                        <span style="color:#ffffff;font-size:18px;font-weight:bold;">Bank Operations</span><br/>
                                        <span style="color:#bff0c9;font-size:13px;">{WebUtility.HtmlEncode(title)}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="background-color:#ffffff;border-radius:0 0 8px 8px;padding:24px;">
                                        {contentHtml}
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:16px 8px;text-align:center;">
                                        <span style="font-size:12px;color:#999999;">This is an automated message — please do not reply.</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
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
