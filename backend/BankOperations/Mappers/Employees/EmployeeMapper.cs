using BankOperations.DTOs.Employees;
using BankOperations.Entities;

namespace BankOperations.Mappers.Employees;

public static class EmployeeMapper
{
    public static EmployeeResponseDto ToDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email!,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
