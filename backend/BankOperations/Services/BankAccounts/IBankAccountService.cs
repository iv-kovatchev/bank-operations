using BankOperations.DTOs.BankAccounts;

namespace BankOperations.Services.BankAccounts;

public interface IBankAccountService
{
    Task<IEnumerable<BankAccountResponseDto>> GetAllByClientIdAsync(Guid clientId);
    Task<BankAccountResponseDto> OpenAccountAsync(Guid clientId, CreateBankAccountDto dto, Guid createdByUserId);
    Task CloseAccountAsync(Guid id, Guid requestingUserId, bool isAdmin);
    Task DeleteAccountAsync(Guid id);
    Task<BankAccountResponseDto> DepositAsync(Guid id, decimal amount, Guid requestingUserId, bool isAdmin);
    Task<BankAccountResponseDto> WithdrawAsync(Guid id, decimal amount, Guid requestingUserId, bool isAdmin);
}
