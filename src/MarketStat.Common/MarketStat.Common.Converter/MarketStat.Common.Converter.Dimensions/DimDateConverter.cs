using MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.Dimensions;

public static class DimDateConverter
{
    public static DimDateDbModel ToDbModel(DimDate dimDate)
    {
        ArgumentNullException.ThrowIfNull(dimDate);

        return new DimDateDbModel
        {
            DateId = dimDate.DateId,
            FullDate = dimDate.FullDate,
            Year = dimDate.Year,
            Quarter = dimDate.Quarter,
            Month = dimDate.Month,
        };
    }

    public static DimDate ToDomain(DimDateDbModel dbDate)
    {
        ArgumentNullException.ThrowIfNull(dbDate);

        return new DimDate(
            dbDate.DateId,
            dbDate.FullDate,
            dbDate.Year,
            dbDate.Quarter,
            dbDate.Month);
    }
}
