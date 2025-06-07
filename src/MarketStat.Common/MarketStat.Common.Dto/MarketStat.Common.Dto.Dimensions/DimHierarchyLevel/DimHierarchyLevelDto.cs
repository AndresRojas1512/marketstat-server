using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimHierarchyLevel;

public record DimHierarchyLevelDto
{
    [JsonPropertyName("hierarchyLevelId")]
    public int HierarchyLevelId { get; init; }
        
    [JsonPropertyName("hierarchyLevelCode")]
    public string HierarchyLevelCode { get; init; } = string.Empty;

    [JsonPropertyName("hierarchyLevelName")]
    public string HierarchyLevelName { get; init; } = string.Empty;
}