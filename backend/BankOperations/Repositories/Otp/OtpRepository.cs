using BankOperations.Data;
using BankOperations.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Repositories.Otp;

public class OtpRepository : IOtpRepository
{
    private readonly ApplicationDbContext _context;

    public OtpRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(OtpCode otp)
    {
        await _context.OtpCodes.AddAsync(otp);
        await _context.SaveChangesAsync();
    }

    public async Task<OtpCode?> GetValidAsync(Guid userId, string code)
        => await _context.OtpCodes
            .FirstOrDefaultAsync(o =>
                o.UserId == userId &&
                o.Code == code &&
                !o.IsUsed &&
                o.ExpiresAt > DateTime.UtcNow);

    public async Task MarkAsUsedAsync(Guid id)
    {
        var otp = await _context.OtpCodes.FindAsync(id);

        if (otp is null)
            return;

        otp.IsUsed = true;
        await _context.SaveChangesAsync();
    }

    public async Task InvalidateAllForUserAsync(Guid userId)
    {
        await _context.OtpCodes
            .Where(o => o.UserId == userId && !o.IsUsed)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.IsUsed, true));
    }
}
