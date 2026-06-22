using BankOperations.Services.ActivityLogs;
using BankOperations.Services.Auth;
using BankOperations.Services.BankAccounts;
using BankOperations.Services.Clients;
using BankOperations.Services.Clients.CorporateClients;
using BankOperations.Services.Clients.IndividualClients;
using BankOperations.Services.Credits;
using BankOperations.Services.CreditServices;
using BankOperations.Services.Email;
using BankOperations.Services.Employees;
using BankOperations.Services.Otp;
using BankOperations.Services.Password;
using BankOperations.Services.Settings;
using BankOperations.Services.Stats;
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
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<ICreditServiceService, CreditServiceService>();
        services.AddScoped<ICreditService, CreditService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IStatsService, StatsService>();
        return services;
    }
}
