using System.ComponentModel.DataAnnotations;
using BankOperations.Enums;

namespace BankOperations.DTOs.Credits.MortgageCredits;

public class CreateMortgageCreditDto
{
    public Guid ClientId { get; set; }
    public Guid CreditServiceId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Range(1, 600, ErrorMessage = "Term must be between 1 and 600 months")]
    public int TermMonths { get; set; }

    [Required]
    public string PropertyAddress { get; set; } = string.Empty;

    public PropertyType PropertyType { get; set; }
}
