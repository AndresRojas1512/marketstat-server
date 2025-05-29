using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class PublicRoleByLocationIndustryDto
{
    [JsonPropertyName("standardJobRoleTitle")]
    public string? StandardJobRoleTitle { get; set; }

    [JsonPropertyName("averageSalary")]
    public decimal? AverageSalary { get; set; }

    [JsonPropertyName("salaryRecordCount")]
    public long SalaryRecordCount { get; set; }
}