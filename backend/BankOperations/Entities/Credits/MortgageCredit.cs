using BankOperations.Enums;

namespace BankOperations.Entities.Credits;

public class MortgageCredit : Credit
{
    public string PropertyAddress { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
}
