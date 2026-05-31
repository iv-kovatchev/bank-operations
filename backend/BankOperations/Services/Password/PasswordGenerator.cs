using System.Security.Cryptography;

namespace BankOperations.Services.Password;

public class PasswordGenerator : IPasswordGenerator
{
    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lower = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Special = "!@#$%";
    private const string All = Upper + Lower + Digits + Special;

    public string GeneratePassword()
    {
        var chars = new char[12];

        chars[0] = Upper[RandomNumberGenerator.GetInt32(Upper.Length)];
        chars[1] = Lower[RandomNumberGenerator.GetInt32(Lower.Length)];
        chars[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
        chars[3] = Special[RandomNumberGenerator.GetInt32(Special.Length)];

        for (int i = 4; i < 12; i++)
            chars[i] = All[RandomNumberGenerator.GetInt32(All.Length)];

        for (int i = 11; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
