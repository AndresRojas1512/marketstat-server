using MarketStat.Common.Enums;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class BenchmarkQueryDto
{
    public string? IndustryFieldNameFilter { get; set; }
    public string? StandardJobRoleTitleFilter { get; set; }
    public string? HierarchyLevelNameFilter { get; set; }
    public string? DistrictNameFilter { get; set; }
    public string? OblastNameFilter { get; set; }
    public string? CityNameFilter { get; set; }
    
    public DateOnly? DateStart { get; set; }
    public DateOnly? DateEnd { get; set; }

    public int TargetPercentile { get; set; } = 90;

    public TimeGranularity Granularity { get; set; } = TimeGranularity.Month;
    public int Periods { get; set; } = 12;
}