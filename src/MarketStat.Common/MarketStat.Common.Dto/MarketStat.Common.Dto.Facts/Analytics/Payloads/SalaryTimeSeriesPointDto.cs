namespace MarketStat.Common.Dto.Facts.Analytics.Payloads;

using System.Text.Json.Serialization;

public class SalaryTimeSeriesPointDto
{
    [JsonPropertyName("period_start")]
    public DateOnly PeriodStart { get; set; }

    [JsonPropertyName("avg_salary")]
    public decimal? AvgSalary { get; set; }

    [JsonPropertyName("salary_count_in_period")]
    public long SalaryCountInPeriod { get; set; }
}
