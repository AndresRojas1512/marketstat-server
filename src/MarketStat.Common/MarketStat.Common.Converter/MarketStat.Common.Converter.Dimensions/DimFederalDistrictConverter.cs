using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimFederalDistrictConverter
{
    public static DimFederalDistrictDbModel ToDbModel(DimFederalDistrict dimDistrict)
    {
        return new DimFederalDistrictDbModel(
            dimDistrict.DistrictId,
            dimDistrict.DistrictName
        );
    }

    public static DimFederalDistrict ToDomain(DimFederalDistrictDbModel dbDistrict)
    {
        return new DimFederalDistrict(
            dbDistrict.DistrictId,
            dbDistrict.DistrictName
        );
    }
}