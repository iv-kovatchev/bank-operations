using BankOperations.DTOs.Employees;

namespace BankOperations.Services.Employees;

public interface IEmployeeService
{
    Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto, Guid createdByUserId);
    Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync();
    Task<EmployeeResponseDto> GetEmployeeByIdAsync(Guid id);
    Task DeactivateEmployeeAsync(Guid id, Guid requestingAdminId);
    Task ActivateEmployeeAsync(Guid id, Guid requestingAdminId);
}
