using BankOperations.Data;
using BankOperations.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BankOperations.Repositories.Employees;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeeRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id)
        => await _context.Users.FindAsync(id);

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        => await _userManager.GetUsersInRoleAsync("Employee");

    public async Task AddAsync(ApplicationUser entity)
        => await _context.Users.AddAsync(entity);

    public async Task UpdateAsync(ApplicationUser entity)
        => _context.Users.Update(entity);

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) _context.Users.Remove(entity);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<bool> ExistsByEmailAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email);
}
