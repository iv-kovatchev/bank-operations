using BankOperations.DTOs.Credits;
using BankOperations.DTOs.Credits.ConsumerCredits;
using BankOperations.DTOs.Credits.MortgageCredits;

namespace BankOperations.Services.Credits;

public interface ICreditService
{
    Task<IEnumerable<CreditResponseDto>> GetAllByClientIdAsync(Guid clientId, Guid requestingUserId, bool isAdmin);
    Task<CreditResponseDto> GetByIdAsync(Guid id, Guid requestingUserId, bool isAdmin);
    Task<CreditResponseDto> GrantConsumerCreditAsync(CreateConsumerCreditDto dto, Guid createdByUserId);
    Task<CreditResponseDto> GrantMortgageCreditAsync(CreateMortgageCreditDto dto, Guid createdByUserId);
    Task<CreditResponseDto> UpdateConsumerCreditAsync(Guid id, UpdateConsumerCreditDto dto, Guid requestingUserId, bool isAdmin);
    Task<CreditResponseDto> UpdateMortgageCreditAsync(Guid id, UpdateMortgageCreditDto dto, Guid requestingUserId, bool isAdmin);
    Task<RepaymentPlanResponseDto> GetRepaymentPlanAsync(Guid creditId, Guid requestingUserId, bool isAdmin);
}
