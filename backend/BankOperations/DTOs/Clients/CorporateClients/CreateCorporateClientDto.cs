using System.ComponentModel.DataAnnotations;

namespace BankOperations.DTOs.Clients.CorporateClients;

public class CreateCorporateClientDto
{
    [Required(ErrorMessage = "Company name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 200 characters.")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "EIK is required.")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "EIK must be exactly 9 digits.")]
    public string EIK { get; set; } = string.Empty;

    [Required(ErrorMessage = "Representative first name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Representative first name must be between 2 and 100 characters.")]
    public string RepresentativeFirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Representative last name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Representative last name must be between 2 and 100 characters.")]
    public string RepresentativeLastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;
}
