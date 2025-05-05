namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimStandardJobRoleHierarchy
{
    public int StandardJobRoleId { get; set; }
    public int HierarchyLevelId { get; set; }

    public DimStandardJobRoleHierarchy(int standardJobRoleId, int hierarchyLevelId)
    {
        StandardJobRoleId = standardJobRoleId;
        HierarchyLevelId = hierarchyLevelId;
    }
}