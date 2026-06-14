using BankOperations.Data;
using BankOperations.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Repositories.CreditServices;

public class CreditServiceRepository : ICreditServiceRepository
{
    private readonly ApplicationDbContext _context;

    public CreditServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreditService?> GetByIdAsync(Guid id)
        => await _context.CreditServices.FindAsync(id);

    public async Task<IEnumerable<CreditService>> GetAllAsync()
        => await _context.CreditServices.ToListAsync();

    public async Task AddAsync(CreditService entity)
        => await _context.CreditServices.AddAsync(entity);

    public async Task UpdateAsync(CreditService entity)
        => _context.CreditServices.Update(entity);

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) _context.CreditServices.Remove(entity);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<bool> ExistsByNameAsync(string name)
        => await _context.CreditServices.AnyAsync(cs => cs.Name == name);
}
