using BankOperations.Services.Auth;
using BankOperations.Services.Clients;
using BankOperations.Services.Clients.CorporateClients;
using BankOperations.Services.Clients.IndividualClients;
using BankOperations.Services.Email;
using BankOperations.Services.Otp;
using BankOperations.Services.Password;
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
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IIndividualClientService, IndividualClientService>();
        services.AddScoped<ICorporateClientService, CorporateClientService>();
        services.AddScoped<IPasswordGenerator, PasswordGenerator>();
        return services;
    }
}
