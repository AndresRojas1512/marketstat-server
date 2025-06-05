namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimHierarchyLevel
{
    public int HierarchyLevelId { get; set; }
    public string HierarchyLevelName { get; set; }

    public DimHierarchyLevel()
    {
    }

    public DimHierarchyLevel(int hierarchyLevelId, string hierarchyLevelName)
    {
        HierarchyLevelId = hierarchyLevelId;
        HierarchyLevelName = hierarchyLevelName;
    }
}