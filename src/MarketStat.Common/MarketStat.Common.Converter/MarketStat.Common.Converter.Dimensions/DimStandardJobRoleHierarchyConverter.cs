using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimStandardJobRoleHierarchyConverter
{
    public static DimStandardJobRoleHierarchyDbModel ToDbModel(DimStandardJobRoleHierarchy link)
    {
        return new DimStandardJobRoleHierarchyDbModel(
            link.StandardJobRoleId,
            link.HierarchyLevelId
        );
    }

    public static DimStandardJobRoleHierarchy ToDomain(DimStandardJobRoleHierarchyDbModel dbLink)
    {
        return new DimStandardJobRoleHierarchy(
            dbLink.StandardJobRoleId,
            dbLink.HierarchyLevelId
        );
    }
}