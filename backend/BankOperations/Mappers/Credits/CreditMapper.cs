using BankOperations.DTOs.CreditServices;
using BankOperations.Entities;

namespace BankOperations.Mappers.Credits;

public static class CreditMapper
{
    public static CreditServiceResponseDto ToDto(CreditService creditService)
    {
        return new CreditServiceResponseDto
        {
            Id = creditService.Id,
            Name = creditService.Name,
            Type = creditService.Type.ToString(),
            InterestRate = creditService.InterestRate,
            MaxAmount = creditService.MaxAmount,
            MaxTermMonths = creditService.MaxTermMonths
        };
    }
}
