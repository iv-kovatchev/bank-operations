using BankOperations.Data;
using BankOperations.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Repositories.BankAccounts;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly ApplicationDbContext _context;

    public BankAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccount?> GetByIdAsync(Guid id)
        => await _context.BankAccounts.FirstOrDefaultAsync(ba => ba.Id == id && !ba.IsDeleted);

    public async Task<IEnumerable<BankAccount>> GetAllAsync()
        => await _context.BankAccounts.ToListAsync();

    public async Task AddAsync(BankAccount entity)
        => await _context.BankAccounts.AddAsync(entity);

    public async Task UpdateAsync(BankAccount entity)
        => _context.BankAccounts.Update(entity);

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return;
        entity.IsDeleted = true;
        _context.BankAccounts.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<IEnumerable<BankAccount>> GetAllByClientIdAsync(Guid clientId)
        => await _context.BankAccounts
            .Include(ba => ba.CreatedByUser)
            .Where(ba => ba.ClientId == clientId && !ba.IsDeleted)
            .ToListAsync();

    public async Task<bool> ExistsByIbanAsync(string iban)
        => await _context.BankAccounts.AnyAsync(ba => ba.IBAN == iban);

    public async Task<BankAccount?> GetByIdWithClientAsync(Guid id)
        => await _context.BankAccounts
            .Include(ba => ba.Client)
            .FirstOrDefaultAsync(ba => ba.Id == id && !ba.IsDeleted);
}
