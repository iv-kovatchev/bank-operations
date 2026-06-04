using System.ComponentModel.DataAnnotations;

namespace BankOperations.DTOs.BankAccounts;

public class CreateBankAccountDto
{
    [Required]
    public string IBAN { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative")]
    public decimal InitialBalance { get; set; }
}
