namespace BankOperations.DTOs.Stats;

public class StatsResponseDto
{
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int TotalBankAccounts { get; set; }
    public int ActiveBankAccounts { get; set; }
    public int TotalCredits { get; set; }
    public int ActiveCredits { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal TotalBalance { get; set; }
}
