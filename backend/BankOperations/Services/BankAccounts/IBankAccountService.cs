using BankOperations.DTOs.BankAccounts;

namespace BankOperations.Services.BankAccounts;

public interface IBankAccountService
{
    Task<IEnumerable<BankAccountResponseDto>> GetAllByClientIdAsync(Guid clientId);
    Task<BankAccountResponseDto> OpenAccountAsync(Guid clientId, CreateBankAccountDto dto, Guid createdByUserId);
    Task CloseAccountAsync(Guid id);
}
