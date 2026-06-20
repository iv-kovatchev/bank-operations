using System.ComponentModel.DataAnnotations;

namespace BankOperations.DTOs.Credits;

public class PayInstallmentDto
{
    [Required]
    public Guid BankAccountId { get; set; }
}
