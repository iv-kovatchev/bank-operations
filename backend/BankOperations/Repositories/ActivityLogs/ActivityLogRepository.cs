using BankOperations.Data;
using BankOperations.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Repositories.ActivityLogs;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityLog?> GetByIdAsync(Guid id)
        => await _context.ActivityLogs.FindAsync(id);

    public async Task<IEnumerable<ActivityLog>> GetAllAsync()
        => await _context.ActivityLogs
            .Include(al => al.User)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync();

    public async Task AddAsync(ActivityLog entity)
        => await _context.ActivityLogs.AddAsync(entity);

    public async Task UpdateAsync(ActivityLog entity)
        => _context.ActivityLogs.Update(entity);

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) _context.ActivityLogs.Remove(entity);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
