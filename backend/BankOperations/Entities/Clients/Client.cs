using BankOperations.Entities.Credits;

namespace BankOperations.Entities.Clients;

public class Client
{
    public Guid ClientId { get; set; }
    public Guid CreatedByUserId { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
    public ICollection<Credit> Credits { get; set; } = new List<Credit>();
}
