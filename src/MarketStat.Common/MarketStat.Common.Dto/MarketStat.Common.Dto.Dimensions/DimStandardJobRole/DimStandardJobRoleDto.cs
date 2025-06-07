using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimStandardJobRole;

public record DimStandardJobRoleDto
{
    [JsonPropertyName("standardJobRoleId")]
    public int StandardJobRoleId { get; init; }

    [JsonPropertyName("standardJobRoleCode")]
    public string StandardJobRoleCode { get; init; } = string.Empty;

    [JsonPropertyName("standardJobRoleTitle")]
    public string StandardJobRoleTitle { get; init; } = string.Empty;

    [JsonPropertyName("industryFieldId")]
    public int IndustryFieldId { get; init; }
}