using BankOperations.Data;
using BankOperations.Entities;
using BankOperations.Entities.Credits;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Repositories.Credits;

public class CreditRepository : ICreditRepository
{
    private readonly ApplicationDbContext _context;

    public CreditRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Credit?> GetByIdAsync(Guid id)
        => await _context.Credits.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Credit>> GetAllAsync()
        => await _context.Credits.ToListAsync();

    public async Task AddAsync(Credit entity)
        => await _context.Credits.AddAsync(entity);

    public async Task UpdateAsync(Credit entity)
        => _context.Credits.Update(entity);

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) _context.Credits.Remove(entity);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<IEnumerable<Credit>> GetAllByClientIdAsync(Guid clientId)
        => await _context.Credits
            .Include(c => c.Client)
            .Include(c => c.CreditService)
            .Where(c => c.ClientId == clientId)
            .ToListAsync();

    public async Task<Credit?> GetByIdWithDetailsAsync(Guid id)
        => await _context.Credits
            .Include(c => c.Client)
            .Include(c => c.CreditService)
            .Include(c => c.RepaymentPlan)
                .ThenInclude(rp => rp!.Installments.OrderBy(i => i.InstallmentNumber))
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<RepaymentPlan?> GetRepaymentPlanAsync(Guid creditId)
        => await _context.RepaymentPlans
            .Include(rp => rp.Installments.OrderBy(i => i.InstallmentNumber))
            .FirstOrDefaultAsync(rp => rp.CreditId == creditId);

}
