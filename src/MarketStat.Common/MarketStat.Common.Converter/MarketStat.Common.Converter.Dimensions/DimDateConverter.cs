using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimDateConverter
{
    public static DimDateDbModel ToDbModel(DimDate dimDate)
    {
        if (dimDate == null)
            throw new ArgumentNullException(nameof(dimDate));

        return new DimDateDbModel
        {
            DateId = dimDate.DateId,
            FullDate = dimDate.FullDate,
            Year = dimDate.Year,
            Quarter = dimDate.Quarter,
            Month = dimDate.Month
        };
    }

    public static DimDate ToDomain(DimDateDbModel dbDate)
    {
        if (dbDate == null)
            throw new ArgumentNullException(nameof(dbDate));
        return new DimDate(
            dbDate.DateId,
            dbDate.FullDate,
            dbDate.Year,
            dbDate.Quarter,
            dbDate.Month
        );
    }
}