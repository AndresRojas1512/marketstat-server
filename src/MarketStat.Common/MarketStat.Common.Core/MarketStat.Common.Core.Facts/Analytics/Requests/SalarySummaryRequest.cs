namespace MarketStat.Common.Core.Facts.Analytics.Requests;

public class SalarySummaryRequest : AnalysisFilterRequest
{
    public int TargetPercentile { get; set; }
}
