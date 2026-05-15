using BankOperations.Enums;

namespace BankOperations.Entities.Credits;

public class ConsumerCredit : Credit
{
    public CreditPurpose Purpose { get; set; }
}
