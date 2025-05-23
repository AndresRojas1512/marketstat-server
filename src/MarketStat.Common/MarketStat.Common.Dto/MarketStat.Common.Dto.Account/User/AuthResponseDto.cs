namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserDto User { get; set; } = null!;
}