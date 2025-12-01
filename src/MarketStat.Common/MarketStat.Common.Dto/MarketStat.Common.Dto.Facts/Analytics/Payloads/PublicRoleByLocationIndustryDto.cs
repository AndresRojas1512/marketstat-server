namespace MarketStat.Common.Dto.Facts.Analytics.Payloads;

public class PublicRoleByLocationIndustryDto
{
    public string StandardJobRoleTitle { get; set; } = string.Empty;

    public decimal AverageSalary { get; set; }

    public long SalaryRecordCount { get; set; }
}
