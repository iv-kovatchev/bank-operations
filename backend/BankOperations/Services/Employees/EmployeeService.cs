using BankOperations.DTOs.Employees;
using BankOperations.Entities;
using BankOperations.Exceptions;
using BankOperations.Mappers.Employees;
using BankOperations.Repositories.Employees;
using BankOperations.Services.Email;
using BankOperations.Services.Password;
using Microsoft.AspNetCore.Identity;

namespace BankOperations.Services.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IEmailService _emailService;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        UserManager<ApplicationUser> userManager,
        IPasswordGenerator passwordGenerator,
        IEmailService emailService)
    {
        _employeeRepository = employeeRepository;
        _userManager = userManager;
        _passwordGenerator = passwordGenerator;
        _emailService = emailService;
    }

    public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, Guid createdByUserId)
    {
        if (await _employeeRepository.ExistsByEmailAsync(dto.Email))
            throw new ConflictException("An employee with this email already exists.");

        var password = "Employee@123";

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Employee");

        await _emailService.SendWelcomeEmailAsync(dto.Email, dto.FirstName, password);

        return EmployeeMapper.ToDto(user);
    }

    public async Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return employees.Select(EmployeeMapper.ToDto);
    }

    public async Task<EmployeeResponseDto> GetEmployeeByIdAsync(Guid id)
    {
        var user = await GetEmployeeUserAsync(id);
        return EmployeeMapper.ToDto(user);
    }

    public async Task DeactivateEmployeeAsync(Guid id, Guid requestingAdminId)
    {
        var user = await GetEmployeeUserAsync(id);

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
    }

    public async Task ActivateEmployeeAsync(Guid id, Guid requestingAdminId)
    {
        var user = await GetEmployeeUserAsync(id);

        user.IsActive = true;
        await _userManager.UpdateAsync(user);
    }

    private async Task<ApplicationUser> GetEmployeeUserAsync(Guid id)
    {
        var user = await _employeeRepository.GetByIdAsync(id);
        if (user == null || !await _userManager.IsInRoleAsync(user, "Employee"))
            throw new NotFoundException("Employee", id);

        return user;
    }
}
