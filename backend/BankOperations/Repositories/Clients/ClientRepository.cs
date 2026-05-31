using BankOperations.Data;
using BankOperations.Entities.Clients;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Repositories.Clients;

public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _context;

    public ClientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id)
        => await _context.Clients.FindAsync(id);

    public async Task<IEnumerable<Client>> GetAllAsync()
        => await _context.Clients.ToListAsync();

    public async Task AddAsync(Client entity)
        => await _context.Clients.AddAsync(entity);

    public async Task UpdateAsync(Client entity)
        => _context.Clients.Update(entity);

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) _context.Clients.Remove(entity);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<Client?> GetByIdWithDetailsAsync(Guid id)
        => await _context.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.ClientId == id);

    public async Task<IEnumerable<Client>> GetAllWithDetailsAsync()
        => await _context.Clients
            .Include(c => c.User)
            .ToListAsync();

    public async Task<bool> ExistsByEmailAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email);

    public async Task<bool> ExistsByEGNAsync(string egn)
        => await _context.IndividualClients.AnyAsync(c => c.EGN == egn);

    public async Task<bool> ExistsByEIKAsync(string eik)
        => await _context.CorporateClients.AnyAsync(c => c.EIK == eik);
}
