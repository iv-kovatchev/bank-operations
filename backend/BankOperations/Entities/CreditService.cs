using BankOperations.Entities.Credits;
using BankOperations.Enums;

namespace BankOperations.Entities;

public class CreditService : BaseEntity
{
    public CreditType Type { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MaxAmount { get; set; }
    public int MaxTermMonths { get; set; }

    // Navigation properties
    public ICollection<Credit> Credits { get; set; } = new List<Credit>();
}
