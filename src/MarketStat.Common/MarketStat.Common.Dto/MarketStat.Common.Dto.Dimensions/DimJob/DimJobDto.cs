using System.Text.Json.Serialization;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimJob;

public record DimJobDto
{
    [JsonPropertyName("jobId")]
    public int JobId { get; init; }

    [JsonPropertyName("jobRoleTitle")]
    public string JobRoleTitle { get; init; } = string.Empty;
    
    [JsonPropertyName("standardJobRoleTitle")]
    public string StandardJobRoleTitle { get; init; } = string.Empty;
    
    [JsonPropertyName("hierarchyLevelName")]
    public string HierarchyLevelName { get; init; } = string.Empty;
    
    [JsonPropertyName("industryFieldId")]
    public int IndustryFieldId { get; init; }
    
    [JsonPropertyName("industryField")]
    public DimIndustryFieldDto? IndustryField { get; init; }
}