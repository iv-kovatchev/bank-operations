using System.ComponentModel.DataAnnotations;
using BankOperations.Enums;

namespace BankOperations.DTOs.CreditServices;

public class CreateCreditServiceDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Type is required.")]
    public CreditType Type { get; set; }

    [Range(0.01, 100, ErrorMessage = "Interest rate must be between 0.01 and 100.")]
    public decimal InterestRate { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Max amount must be greater than 0.")]
    public decimal MaxAmount { get; set; }

    [Range(1, 600, ErrorMessage = "Max term must be between 1 and 600 months.")]
    public int MaxTermMonths { get; set; }
}
