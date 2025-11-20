namespace MarketStat.Common.Core.MarketStat.Common.Core.Account;

public class AuthResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public required User User { get; set; }
}