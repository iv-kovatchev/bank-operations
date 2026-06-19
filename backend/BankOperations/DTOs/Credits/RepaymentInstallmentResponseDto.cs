namespace BankOperations.DTOs.Credits;

public class RepaymentInstallmentResponseDto
{
    public Guid Id { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalPart { get; set; }
    public decimal InterestPart { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IsPaid { get; set; }
}
