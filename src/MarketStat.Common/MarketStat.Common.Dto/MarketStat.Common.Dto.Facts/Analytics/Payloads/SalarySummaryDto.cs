using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;

public class SalarySummaryDto
{
    [JsonPropertyName("percentile25")]
    public decimal? Percentile25 { get; set; }

    [JsonPropertyName("percentile50")]
    public decimal? Percentile50 { get; set; }

    [JsonPropertyName("percentile75")]
    public decimal? Percentile75 { get; set; }

    [JsonPropertyName("percentile_target")]
    public decimal? PercentileTarget { get; set; }

    [JsonPropertyName("average_salary")]
    public decimal? AverageSalary { get; set; }

    [JsonPropertyName("total_count")]
    public long TotalCount { get; set; }

    public SalarySummaryDto()
    {
        TotalCount = 0;
    }
}