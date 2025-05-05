using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimDateConverter
{
    public static DimDateDbModel ToDbModel(DimDate dimDate)
    {
        return new DimDateDbModel(
            dimDate.DateId,
            dimDate.FullDate,
            dimDate.Year,
            dimDate.Quarter,
            dimDate.Month
        );
    }

    public static DimDate ToDomain(DimDateDbModel dbDate)
    {
        return new DimDate(
            dbDate.DateId,
            dbDate.FullDate,
            dbDate.Year,
            dbDate.Quarter,
            dbDate.Month
        );
    }
}