namespace BankOperations.DTOs.Clients;

public abstract class ClientResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
}
