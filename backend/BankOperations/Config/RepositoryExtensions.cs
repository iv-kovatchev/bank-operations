using BankOperations.Repositories.Otp;
using BankOperations.Repositories.Token;

namespace BankOperations.Config;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        return services;
    }
}
