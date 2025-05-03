using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimStandardJobRoleConverter
{
    public static DimStandardJobRoleDbModel ToDbModel(DimStandardJobRole jobRole)
    {
        return new DimStandardJobRoleDbModel(
            jobRole.StandardJobRoleId,
            jobRole.StandardJobRoleTitle,
            jobRole.IndustryFieldId
        );
    }

    public static DimStandardJobRole ToDomain(DimStandardJobRoleDbModel dbJobRole)
    {
        return new DimStandardJobRole(
            dbJobRole.StandardJobRoleId,
            dbJobRole.StandardJobRoleTitle,
            dbJobRole.IndustryFieldId
        );
    }
}