namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;

public class AnalysisFilterRequest
{
    public string? StandardJobRoleTitle { get; set; }
    public string? HierarchyLevelName { get; set; }
    public string? IndustryFieldName { get; set; }
    
    public string? DistrictName { get; set; }
    public string? OblastName { get; set; }
    public string? CityName { get; set; }
    
    public DateOnly? DateStart { get; set; }
    public DateOnly? DateEnd { get; set; }
}