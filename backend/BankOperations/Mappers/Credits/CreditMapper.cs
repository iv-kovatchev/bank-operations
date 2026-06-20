using BankOperations.DTOs.CreditServices;
using BankOperations.DTOs.Credits;
using BankOperations.Entities;
using BankOperations.Entities.Credits;

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

    public static CreditResponseDto ToDto(ConsumerCredit credit) => new()
    {
        Id = credit.Id,
        ClientId = credit.ClientId,
        CreditServiceId = credit.CreditServiceId,
        CreditType = "Consumer",
        Amount = credit.Amount,
        TermMonths = credit.TermMonths,
        Status = credit.Status.ToString(),
        CreatedAt = credit.CreatedAt,
        CreatedByUserId = credit.CreatedByUserId,
        Purpose = credit.Purpose.ToString()
    };

    public static CreditResponseDto ToDto(MortgageCredit credit) => new()
    {
        Id = credit.Id,
        ClientId = credit.ClientId,
        CreditServiceId = credit.CreditServiceId,
        CreditType = "Mortgage",
        Amount = credit.Amount,
        TermMonths = credit.TermMonths,
        Status = credit.Status.ToString(),
        CreatedAt = credit.CreatedAt,
        CreatedByUserId = credit.CreatedByUserId,
        PropertyAddress = credit.PropertyAddress,
        PropertyType = credit.PropertyType.ToString()
    };

    public static CreditResponseDto ToDto(Credit credit) => credit switch
    {
        ConsumerCredit cc => ToDto(cc),
        MortgageCredit mc => ToDto(mc),
        _ => throw new ArgumentException("Unknown credit type")
    };

    public static RepaymentPlanResponseDto ToDto(RepaymentPlan plan)
    {
        return new RepaymentPlanResponseDto
        {
            CreditId = plan.CreditId,
            MonthlyInstallment = plan.MonthlyInstallment,
            GeneratedAt = plan.GeneratedAt,
            Installments = plan.Installments.Select(ToDto).ToList()
        };
    }

    public static RepaymentInstallmentResponseDto ToDto(RepaymentInstallment installment)
    {
        return new RepaymentInstallmentResponseDto
        {
            Id = installment.Id,
            InstallmentNumber = installment.InstallmentNumber,
            DueDate = installment.DueDate,
            PrincipalPart = installment.PrincipalPart,
            InterestPart = installment.InterestPart,
            TotalAmount = installment.PrincipalPart + installment.InterestPart,
            RemainingBalance = installment.RemainingBalance,
            PaidAt = installment.PaidAt,
            IsPaid = installment.PaidAt != null
        };
    }
}
