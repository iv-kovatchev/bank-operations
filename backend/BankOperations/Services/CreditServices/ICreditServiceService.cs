using BankOperations.DTOs.CreditServices;

namespace BankOperations.Services.CreditServices;

public interface ICreditServiceService
{
    Task<IEnumerable<CreditServiceResponseDto>> GetAllAsync();
    Task<CreditServiceResponseDto> GetByIdAsync(Guid id);
    Task<CreditServiceResponseDto> CreateAsync(CreateCreditServiceDto dto);
    Task<CreditServiceResponseDto> UpdateAsync(Guid id, UpdateCreditServiceDto dto);
    Task DeleteAsync(Guid id);
}
