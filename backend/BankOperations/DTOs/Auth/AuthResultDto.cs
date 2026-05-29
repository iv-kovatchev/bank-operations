namespace BankOperations.DTOs.Auth;

public class AuthResultDto
{
    public string? AccessToken { get; set; }
    public bool RequiresOtp { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
}
