using BankOperations.Entities;

namespace BankOperations.Repositories.Otp;

public interface IOtpRepository
{
    Task SaveAsync(OtpCode otp);
    Task<OtpCode?> GetValidAsync(Guid userId, string code);
    Task MarkAsUsedAsync(Guid id);
    Task InvalidateAllForUserAsync(Guid userId);
}
