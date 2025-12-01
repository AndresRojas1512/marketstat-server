namespace MarketStat.Common.Dto.Facts.Analytics.Requests;

using System.ComponentModel.DataAnnotations;

public class SalaryFilterDto
{
    [StringLength(255)]
    public string? StandardJobRoleTitle { get; set; }

    [StringLength(255)]
    public string? HierarchyLevelName { get; set; }

    [StringLength(255)]
    public string? IndustryFieldName { get; set; }

    [StringLength(255)]
    public string? DistrictName { get; set; }

    [StringLength(255)]
    public string? OblastName { get; set; }

    [StringLength(255)]
    public string? CityName { get; set; }

    public DateOnly? DateStart { get; set; }

    public DateOnly? DateEnd { get; set; }
}
