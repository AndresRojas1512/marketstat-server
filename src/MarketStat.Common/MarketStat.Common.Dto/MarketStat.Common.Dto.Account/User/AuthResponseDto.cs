namespace MarketStat.Common.Dto.Account.User;

using System.Text.Json.Serialization;

public class AuthResponseDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("expiration")]
    public DateTime Expiration { get; set; }

    [JsonPropertyName("user")]
    public UserDto User { get; set; } = null!;
}
