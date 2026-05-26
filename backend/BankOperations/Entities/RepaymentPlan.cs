using BankOperations.Entities.Credits;

namespace BankOperations.Entities;

public class RepaymentPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CreditId { get; set; }
    public decimal MonthlyInstallment { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Credit Credit { get; set; } = null!;
    public ICollection<RepaymentInstallment> Installments { get; set; } = new List<RepaymentInstallment>();
}
