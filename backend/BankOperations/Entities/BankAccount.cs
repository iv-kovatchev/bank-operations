using BankOperations.Entities.Clients;
using BankOperations.Enums;

namespace BankOperations.Entities;

public class BankAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string IBAN { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public Guid ClientId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }

    // Navigation properties
    public Client Client { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
}
