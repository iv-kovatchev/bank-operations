using System.ComponentModel.DataAnnotations;

namespace BankOperations.DTOs.BankAccounts;

public class TransactionDto
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
}
