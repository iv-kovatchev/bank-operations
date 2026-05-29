namespace BankOperations.Entities.Clients;

public class IndividualClient : Client
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EGN { get; set; } = string.Empty;
}
