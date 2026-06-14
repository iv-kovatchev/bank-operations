namespace BankOperations.DTOs.CreditServices;

public class CreditServiceResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public decimal MaxAmount { get; set; }
    public int MaxTermMonths { get; set; }
}
