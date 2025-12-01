namespace MarketStat.Common.Core.Facts.Analytics.Requests;

using MarketStat.Common.Enums;

public class TimeSeriesRequest : AnalysisFilterRequest
{
    public TimeGranularity Granularity { get; set; }

    public int Periods { get; set; }
}
