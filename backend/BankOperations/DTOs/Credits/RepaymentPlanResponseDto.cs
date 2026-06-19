namespace BankOperations.DTOs.Credits;

public class RepaymentPlanResponseDto
{
    public Guid CreditId { get; set; }
    public decimal MonthlyInstallment { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<RepaymentInstallmentResponseDto> Installments { get; set; } = [];
}
