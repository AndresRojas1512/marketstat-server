using System.ComponentModel.DataAnnotations;
using MarketStat.Common.Enums;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class BenchmarkQueryDto
{
    public string? StandardJobRoleTitle { get; set; }
    public string? HierarchyLevelName { get; set; }
    public int? IndustryFieldId { get; set; }
    public string? DistrictName { get; set; }
    public string? OblastName { get; set; }
    public string? CityName { get; set; }
    
    public DateOnly? DateStart { get; set; }
    public DateOnly? DateEnd { get; set; }
    
    public int TargetPercentile { get; set; } = 90;
    public TimeGranularity Granularity { get; set; } = TimeGranularity.Month;
    public int Periods { get; set; } = 12;
}