using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimLocation;

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