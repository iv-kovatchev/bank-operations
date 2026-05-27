using BankOperations.Services.Auth;
using BankOperations.Services.Email;
using BankOperations.Services.Otp;
using BankOperations.Services.Token;

namespace BankOperations.Config;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
