using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimLocationConverter
{
    public static DimLocationDbModel ToDbModel(DimLocation domainLocation)
    {
        if (domainLocation == null)
        {
            throw new ArgumentNullException(nameof(domainLocation));
        }

        return new DimLocationDbModel
        {
            LocationId = domainLocation.LocationId,
            CityName = domainLocation.CityName,
            OblastName = domainLocation.OblastName,
            DistrictName = domainLocation.DistrictName
        };
    }

    public static DimLocation ToDomain(DimLocationDbModel dbLocation)
    {
        if (dbLocation == null)
        {
            throw new ArgumentNullException(nameof(dbLocation));
        }

        return new DimLocation
        {
            LocationId = dbLocation.LocationId,
            CityName = dbLocation.CityName,
            OblastName = dbLocation.OblastName,
            DistrictName = dbLocation.DistrictName
        };
    }
}