using BankOperations.Data;
using BankOperations.Enums;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Repositories.Stats;

public class StatsRepository : IStatsRepository
{
    private readonly ApplicationDbContext _context;

    public StatsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalClientsAsync(Guid? createdByUserId)
    {
        var query = _context.Clients.AsQueryable();
        if (createdByUserId.HasValue)
            query = query.Where(c => c.CreatedByUserId == createdByUserId.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetActiveClientsAsync(Guid? createdByUserId)
    {
        var query = _context.Clients.Include(c => c.User).AsQueryable();
        if (createdByUserId.HasValue)
            query = query.Where(c => c.CreatedByUserId == createdByUserId.Value);

        return await query.CountAsync(c => c.User.IsActive);
    }

    public async Task<int> GetTotalBankAccountsAsync(Guid? createdByUserId)
    {
        var query = _context.BankAccounts.Where(ba => !ba.IsDeleted);
        if (createdByUserId.HasValue)
            query = query.Where(ba => ba.Client.CreatedByUserId == createdByUserId.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetActiveBankAccountsAsync(Guid? createdByUserId)
    {
        var query = _context.BankAccounts.Where(ba => !ba.IsDeleted && ba.Status == AccountStatus.Active);
        if (createdByUserId.HasValue)
            query = query.Where(ba => ba.Client.CreatedByUserId == createdByUserId.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetTotalCreditsAsync(Guid? createdByUserId)
    {
        var query = _context.Credits.AsQueryable();
        if (createdByUserId.HasValue)
            query = query.Where(c => c.Client.CreatedByUserId == createdByUserId.Value);

        return await query.CountAsync();
    }

    public async Task<int> GetActiveCreditsAsync(Guid? createdByUserId)
    {
        var query = _context.Credits.Where(c => c.Status == CreditStatus.Active);
        if (createdByUserId.HasValue)
            query = query.Where(c => c.Client.CreatedByUserId == createdByUserId.Value);

        return await query.CountAsync();
    }

    public async Task<decimal> GetTotalCreditAmountAsync(Guid? createdByUserId)
    {
        var query = _context.Credits.Where(c => c.Status == CreditStatus.Active);
        if (createdByUserId.HasValue)
            query = query.Where(c => c.Client.CreatedByUserId == createdByUserId.Value);

        return await query.SumAsync(c => c.Amount);
    }

    public async Task<decimal> GetTotalBalanceAsync(Guid? createdByUserId)
    {
        var query = _context.BankAccounts.Where(ba => !ba.IsDeleted && ba.Status == AccountStatus.Active);
        if (createdByUserId.HasValue)
            query = query.Where(ba => ba.Client.CreatedByUserId == createdByUserId.Value);

        return await query.SumAsync(ba => ba.Balance);
    }
}
