using BankOperations.Entities;

namespace BankOperations.Repositories.Token;

public interface IRefreshTokenRepository
{
    Task SaveAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task RevokeAsync(string token);
    Task RevokeAllForUserAsync(Guid userId);
}
