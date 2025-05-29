using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class SalaryTimeSeriesPointDto
{
    [JsonPropertyName("period_start")] // Matches JSON key from PG
    public DateOnly PeriodStart { get; set; } // System.Text.Json handles "YYYY-MM-DD" to DateOnly

    [JsonPropertyName("avg_salary")]   // Matches JSON key from PG
    public decimal? AvgSalary { get; set; }

    [JsonPropertyName("salary_count_in_period")] // Matches JSON key from PG
    public long SalaryCountInPeriod { get; set; }
}