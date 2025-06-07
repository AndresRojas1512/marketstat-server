using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimHierarchyLevelConverter
{
    public static DimHierarchyLevelDbModel ToDbModel(DimHierarchyLevel domainHierarchyLevel)
    {
        if (domainHierarchyLevel == null)
            throw new ArgumentNullException(nameof(domainHierarchyLevel));

        return new DimHierarchyLevelDbModel
        {
            HierarchyLevelId = domainHierarchyLevel.HierarchyLevelId,
            HierarchyLevelCode = domainHierarchyLevel.HierarchyLevelCode,
            HierarchyLevelName = domainHierarchyLevel.HierarchyLevelName
        };
    }

    public static DimHierarchyLevel ToDomain(DimHierarchyLevelDbModel dbHierarchyLevel)
    {
        if (dbHierarchyLevel == null)
            throw new ArgumentNullException(nameof(dbHierarchyLevel));

        return new DimHierarchyLevel
        {
            HierarchyLevelId = dbHierarchyLevel.HierarchyLevelId,
            HierarchyLevelCode = dbHierarchyLevel.HierarchyLevelCode,
            HierarchyLevelName = dbHierarchyLevel.HierarchyLevelName
        };
    }
}