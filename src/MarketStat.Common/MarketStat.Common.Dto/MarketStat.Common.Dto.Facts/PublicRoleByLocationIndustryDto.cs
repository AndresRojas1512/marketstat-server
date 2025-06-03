using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class PublicRoleByLocationIndustryDto
{
    public string StandardJobRoleTitle { get; set; } = string.Empty;
    public decimal AverageSalary { get; set; }
    public long SalaryRecordCount { get; set; }
}