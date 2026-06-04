namespace BankOperations.DTOs.BankAccounts;

public class BankAccountResponseDto
{
    public Guid Id { get; set; }
    public string IBAN { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
}
