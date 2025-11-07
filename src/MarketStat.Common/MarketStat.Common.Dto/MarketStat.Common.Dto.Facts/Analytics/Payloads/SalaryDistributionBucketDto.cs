using System.Text.Json.Serialization;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts.Analytics.Payloads;

public class SalaryDistributionBucketDto
{
    [JsonPropertyName("lower_bound")]
    public decimal LowerBound { get; set; }

    [JsonPropertyName("upper_bound")]
    public decimal UpperBound { get; set; }

    [JsonPropertyName("bucket_count")]
    public long BucketCount { get; set; }
}