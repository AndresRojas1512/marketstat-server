using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimHierarchyLevelConverter
{
    public static DimHierarchyLevelDbModel ToDbModel(DimHierarchyLevel dimHierarchyLevel)
    {
        return new DimHierarchyLevelDbModel(
            dimHierarchyLevel.HierarchyLevelId,
            dimHierarchyLevel.HierarchyLevelName
        );
    }

    public static DimHierarchyLevel ToDomain(DimHierarchyLevelDbModel dbHierarchyLevel)
    {
        return new DimHierarchyLevel(
            dbHierarchyLevel.HierarchyLevelId,
            dbHierarchyLevel.HierarchyLevelName
        );
    }
}