using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public class BenchmarkDataDto
{
    [JsonPropertyName("salaryDistribution")]
    public List<SalaryDistributionBucketDto> SalaryDistribution { get; set; }
        
    [JsonPropertyName("salarySummary")]
    public SalarySummaryDto? SalarySummary { get; set; }

    [JsonPropertyName("salaryTimeSeries")]
    public List<SalaryTimeSeriesPointDto> SalaryTimeSeries { get; set; }
        
    public BenchmarkDataDto()
    {
        SalaryDistribution = new List<SalaryDistributionBucketDto>();
        SalaryTimeSeries = new List<SalaryTimeSeriesPointDto>();
    }
}