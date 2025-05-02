using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimOblastConverter
{
    public static DimOblastDbModel ToDbModel(DimOblast dimOblast)
    {
        return new DimOblastDbModel(
            dimOblast.OblastId,
            dimOblast.OblastName,
            dimOblast.DistrictId
        );
    }

    public static DimOblast ToDomain(DimOblastDbModel dbOblast)
    {
        return new DimOblast(
            dbOblast.OblastId,
            dbOblast.OblastName,
            dbOblast.DistrictId
        );
    }
}