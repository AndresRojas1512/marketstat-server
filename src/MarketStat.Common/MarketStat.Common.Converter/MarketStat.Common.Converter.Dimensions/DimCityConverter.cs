using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimCityConverter
{
    public static DimCityDbModel ToDbModel(DimCity dimCity)
    {
        return new DimCityDbModel(
            dimCity.CityId,
            dimCity.CityName,
            dimCity.OblastId
        );
    }

    public static DimCity ToDomain(DimCityDbModel dbCity)
    {
        return new DimCity(
            dbCity.CityId,
            dbCity.CityName,
            dbCity.OblastId
        );
    }
}