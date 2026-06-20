namespace BankOperations.DTOs.Credits;

public class CreditResponseDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid CreditServiceId { get; set; }
    public string CreditType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? Purpose { get; set; }
    public string? PropertyAddress { get; set; }
    public string? PropertyType { get; set; }
}
