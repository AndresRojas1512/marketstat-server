using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Account.BenchmarkHistory;

public class BenchmarkHistoryDto
{
    [JsonPropertyName("benchmarkHistoryId")]
    public long BenchmarkHistoryId { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }
    
    [JsonPropertyName("username")] 
    public string? Username { get; set; }

    [JsonPropertyName("benchmarkName")]
    public string? BenchmarkName { get; set; }

    [JsonPropertyName("savedAt")]
    public DateTimeOffset SavedAt { get; set; }

    [JsonPropertyName("filterIndustryFieldId")]
    public int? FilterIndustryFieldId { get; set; }
    
    [JsonPropertyName("filterStandardJobRoleTitle")]
    public string? FilterStandardJobRoleTitle { get; set; }
    
    [JsonPropertyName("filterHierarchyLevelName")]
    public string? FilterHierarchyLevelName { get; set; }
    
    [JsonPropertyName("filterDistrictName")]
    public string? FilterDistrictName { get; set; }
    
    [JsonPropertyName("filterOblastName")]
    public string? FilterOblastName { get; set; }
    
    [JsonPropertyName("filterCityName")]
    public string? FilterCityName { get; set; }
    
    [JsonPropertyName("filterDateStart")]
    public DateOnly? FilterDateStart { get; set; }
    
    [JsonPropertyName("filterDateEnd")]
    public DateOnly? FilterDateEnd { get; set; }
    
    [JsonPropertyName("filterTargetPercentile")]
    public int? FilterTargetPercentile { get; set; }
    
    [JsonPropertyName("filterGranularity")]
    public string? FilterGranularity { get; set; }
    
    [JsonPropertyName("filterPeriods")]
    public int? FilterPeriods { get; set; }

    [JsonPropertyName("benchmarkResultJson")]
    public string? BenchmarkResultJson { get; set; }
}