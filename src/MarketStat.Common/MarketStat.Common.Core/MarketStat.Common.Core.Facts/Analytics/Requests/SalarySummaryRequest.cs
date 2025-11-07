namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;

public class SalarySummaryRequest : AnalysisFilterRequest
{
    public int TargetPercentile { get; set; }
}