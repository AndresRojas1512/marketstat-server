namespace MarketStat.Common.Dto.Dimensions.DimLocation;

using System.Text.Json.Serialization;

public class DimLocationDto
{
    [JsonPropertyName("locationId")]
    public int LocationId { get; init; }

    [JsonPropertyName("cityName")]
    public string CityName { get; init; } = string.Empty;

    [JsonPropertyName("oblastName")]
    public string OblastName { get; init; } = string.Empty;

    [JsonPropertyName("districtName")]
    public string DistrictName { get; init; } = string.Empty;
}
