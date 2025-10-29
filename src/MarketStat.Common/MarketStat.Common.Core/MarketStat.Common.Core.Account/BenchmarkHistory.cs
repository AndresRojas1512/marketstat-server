namespace MarketStat.Common.Core.MarketStat.Common.Core.Account;

public class BenchmarkHistory
{
    public long BenchmarkHistoryId { get; set; }
    public int UserId { get; set; }
    public string? BenchmarkName { get; set; }
    public DateTimeOffset SavedAt { get; set; }

    public int? FilterIndustryFieldId { get; set; }
    public string? FilterStandardJobRoleTitle { get; set; }
    public string? FilterHierarchyLevelName { get; set; }
    public string? FilterDistrictName { get; set; }
    public string? FilterOblastName { get; set; }
    public string? FilterCityName { get; set; }
    
    public DateOnly? FilterDateStart { get; set; }
    public DateOnly? FilterDateEnd { get; set; }
    
    public int? FilterTargetPercentile { get; set; }
    public string? FilterGranularity { get; set; }
    public int? FilterPeriods { get; set; }

    public string BenchmarkResultJson { get; set; }

    public virtual User? User { get; set; }

    public BenchmarkHistory()
    {
        BenchmarkResultJson = "{}";
        SavedAt = DateTimeOffset.UtcNow;
    }

    public BenchmarkHistory(
        long benchmarkHistoryId,
        int userId,
        string? benchmarkName,
        DateTimeOffset savedAt,
        // ID-based filters
        int? filterIndustryFieldId,
        string? filterStandardJobRoleTitle,
        string? filterHierarchyLevelName,
        string? filterDistrictName,
        string? filterOblastName,
        string? filterCityName,
        DateOnly? filterDateStart,
        DateOnly? filterDateEnd,
        int? filterTargetPercentile,
        string? filterGranularity,
        int? filterPeriods,
        string benchmarkResultJson)
    {
        BenchmarkHistoryId = benchmarkHistoryId;
        UserId = userId;
        BenchmarkName = benchmarkName;
        SavedAt = savedAt;
        FilterIndustryFieldId = filterIndustryFieldId;
        FilterStandardJobRoleTitle = filterStandardJobRoleTitle;
        FilterHierarchyLevelName = filterHierarchyLevelName;
        FilterDistrictName = filterDistrictName;
        FilterOblastName = filterOblastName;
        FilterCityName = filterCityName;
        FilterDateStart = filterDateStart;
        FilterDateEnd = filterDateEnd;
        FilterTargetPercentile = filterTargetPercentile;
        FilterGranularity = filterGranularity;
        FilterPeriods = filterPeriods;
        BenchmarkResultJson = benchmarkResultJson ?? throw new ArgumentNullException(nameof(benchmarkResultJson));
    }
}