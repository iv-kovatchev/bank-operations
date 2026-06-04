using BankOperations.DTOs.BankAccounts;
using BankOperations.Entities;

namespace BankOperations.Mappers.BankAccounts;

public static class BankAccountMapper
{
    public static BankAccountResponseDto ToDto(BankAccount account) => new()
    {
        Id = account.Id,
        IBAN = account.IBAN,
        Balance = account.Balance,
        Status = account.Status.ToString(),
        ClientId = account.ClientId,
        CreatedAt = account.CreatedAt,
        CreatedByUserId = account.CreatedByUserId
    };
}
