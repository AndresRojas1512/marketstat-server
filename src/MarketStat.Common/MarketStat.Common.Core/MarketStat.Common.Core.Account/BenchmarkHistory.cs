namespace MarketStat.Common.Core.MarketStat.Common.Core.Account;

public class BenchmarkHistory
{
    public long BenchmarkHistoryId { get; set; }
    public int UserId { get; set; }

    public string? BenchmarkName { get; set; }
    public DateTimeOffset SavedAt { get; set; }

    public int? FilterIndustryFieldId { get; set; }
    public int? FilterStandardJobRoleId { get; set; }
    public int? FilterHierarchyLevelId { get; set; }
    public int? FilterDistrictId { get; set; }
    public int? FilterOblastId { get; set; }
    public int? FilterCityId { get; set; }
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
        int? filterStandardJobRoleId,
        int? filterHierarchyLevelId,
        int? filterDistrictId,
        int? filterOblastId,
        int? filterCityId,
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
        FilterStandardJobRoleId = filterStandardJobRoleId;
        FilterHierarchyLevelId = filterHierarchyLevelId;
        FilterDistrictId = filterDistrictId;
        FilterOblastId = filterOblastId;
        FilterCityId = filterCityId;
        FilterDateStart = filterDateStart;
        FilterDateEnd = filterDateEnd;
        FilterTargetPercentile = filterTargetPercentile;
        FilterGranularity = filterGranularity;
        FilterPeriods = filterPeriods;
        BenchmarkResultJson = benchmarkResultJson ?? throw new ArgumentNullException(nameof(benchmarkResultJson));
    }
}