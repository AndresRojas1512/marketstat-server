namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class SalaryFilterDto
{
    public int? IndustryFieldId { get; set; }
    public int? StandardJobRoleId { get; set; }
    public int? HierarchyLevelId { get; set; }
    public int? DistrictId { get; set; }
    public int? OblastId { get; set; }
    public int? CityId { get; set; }
    
    public DateOnly? DateStart { get; set; }
    public DateOnly? DateEnd { get; set; }
}