using BankOperations.Entities.Clients;
using BankOperations.Enums;

namespace BankOperations.Entities.Credits;

public class Credit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public Guid CreditServiceId { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public CreditStatus Status { get; set; } = CreditStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
    public CreditService CreditService { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public RepaymentPlan? RepaymentPlan { get; set; }
}
