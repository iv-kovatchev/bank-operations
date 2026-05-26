namespace BankOperations.Entities;

public class RepaymentInstallment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RepaymentPlanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalPart { get; set; }
    public decimal InterestPart { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? CreatedByUserId { get; set; }

    // Navigation properties
    public RepaymentPlan RepaymentPlan { get; set; } = null!;
    public ApplicationUser? CreatedByUser { get; set; }
}
