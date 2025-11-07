using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Requests;

public class PublicRolesRequestDto : SalaryFilterDto
{
    [Range(0, int.MaxValue)]
    public int MinRecordCount { get; set; } = 10;
}