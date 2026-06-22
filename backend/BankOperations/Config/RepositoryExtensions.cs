using BankOperations.Repositories.ActivityLogs;
using BankOperations.Repositories.BankAccounts;
using BankOperations.Repositories.Clients;
using BankOperations.Repositories.Credits;
using BankOperations.Repositories.CreditServices;
using BankOperations.Repositories.Employees;
using BankOperations.Repositories.Otp;
using BankOperations.Repositories.Stats;
using BankOperations.Repositories.Token;

namespace BankOperations.Config;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ICreditServiceRepository, CreditServiceRepository>();
        services.AddScoped<ICreditRepository, CreditRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IStatsRepository, StatsRepository>();
        return services;
    }
}
