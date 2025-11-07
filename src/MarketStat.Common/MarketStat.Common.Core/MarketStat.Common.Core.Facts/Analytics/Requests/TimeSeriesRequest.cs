using MarketStat.Common.Enums;

namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts.Analytics.Requests;

public class TimeSeriesRequest : AnalysisFilterRequest
{
    public TimeGranularity Granularity { get; set; }
    public int Periods { get; set; }
}