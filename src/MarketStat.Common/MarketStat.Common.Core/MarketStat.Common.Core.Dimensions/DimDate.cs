namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimDate
{
    public int DateId { get; set; }
    public DateOnly FullDate { get; set; }
    public int Year { get; set; }
    public int Quarter { get; set; }
    public int Month { get; set; }

    public DimDate(int dateId, DateOnly fullDate, int year, int quarter, int month)
    {
        DateId = dateId;
        FullDate = fullDate;
        Year = year;
        Quarter = quarter;
        Month = month;
    }
}