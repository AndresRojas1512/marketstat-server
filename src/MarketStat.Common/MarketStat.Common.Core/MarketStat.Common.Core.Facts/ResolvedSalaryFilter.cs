using System.Diagnostics.CodeAnalysis;

namespace MarketStat.Common.Core.Facts;

public class ResolvedSalaryFilter
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO requires setters for initialization")]
    public IList<int>? LocationIds { get; set; }

    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO requires setters for initialization")]
    public IList<int>? JobIds { get; set; }

    public DateOnly? DateStart { get; set; }

    public DateOnly? DateEnd { get; set; }
}
