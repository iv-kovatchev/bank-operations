namespace BankOperations.DTOs.Clients;

public class ClientResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Type { get; set; } = string.Empty;

    // Individual fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Egn { get; set; }

    // Corporate fields
    public string? CompanyName { get; set; }
    public string? Eik { get; set; }
    public string? RepresentativeFirstName { get; set; }
    public string? RepresentativeLastName { get; set; }
}
