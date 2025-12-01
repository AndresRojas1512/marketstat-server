namespace MarketStat.Common.Dto.Facts.Analytics.Requests;

using System.ComponentModel.DataAnnotations;

public class PublicRolesRequestDto : SalaryFilterDto
{
    [Range(0, int.MaxValue)]
    public int MinRecordCount { get; set; } = 10;
}
