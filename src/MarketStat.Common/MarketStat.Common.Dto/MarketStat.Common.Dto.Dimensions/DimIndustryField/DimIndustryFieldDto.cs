namespace MarketStat.Common.Dto.Dimensions.DimIndustryField;

using System.Text.Json.Serialization;

public record DimIndustryFieldDto
{
    [JsonPropertyName("industryFieldId")]
    public int IndustryFieldId { get; init; }

    [JsonPropertyName("industryFieldCode")]
    public string IndustryFieldCode { get; init; } = string.Empty;

    [JsonPropertyName("industryFieldName")]
    public string IndustryFieldName { get; init; } = string.Empty;
}
