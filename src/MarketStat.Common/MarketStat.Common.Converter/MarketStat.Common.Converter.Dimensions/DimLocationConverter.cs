using MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.Dimensions;

public static class DimLocationConverter
{
    public static DimLocationDbModel ToDbModel(DimLocation domainLocation)
    {
        ArgumentNullException.ThrowIfNull(domainLocation);

        return new DimLocationDbModel
        {
            LocationId = domainLocation.LocationId,
            CityName = domainLocation.CityName,
            OblastName = domainLocation.OblastName,
            DistrictName = domainLocation.DistrictName,
        };
    }

    public static DimLocation ToDomain(DimLocationDbModel dbLocation)
    {
        ArgumentNullException.ThrowIfNull(dbLocation);

        return new DimLocation
        {
            LocationId = dbLocation.LocationId,
            CityName = dbLocation.CityName,
            OblastName = dbLocation.OblastName,
            DistrictName = dbLocation.DistrictName,
        };
    }
}
