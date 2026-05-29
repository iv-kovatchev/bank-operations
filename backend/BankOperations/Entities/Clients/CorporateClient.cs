namespace BankOperations.Entities.Clients;

public class CorporateClient : Client
{
    public string CompanyName { get; set; } = string.Empty;
    public string EIK { get; set; } = string.Empty;
    public string RepresentativeFirstName { get; set; } = string.Empty;
    public string RepresentativeLastName { get; set; } = string.Empty;
}
