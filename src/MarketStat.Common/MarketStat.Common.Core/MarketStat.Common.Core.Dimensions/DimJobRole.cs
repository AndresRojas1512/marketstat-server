namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimJobRole
{
    public int JobRoleId { get; set; }
    public string JobRoleTitle { get; set; }
    public int StandardJobRoleId { get; set; }
    public int HierarchyLevelId { get; set; }
    
    public DimJobRole(int jobRoleId, string jobRoleTitle, int standardJobRoleId, int hierarchyLevelId)
    {
        JobRoleId = jobRoleId;
        JobRoleTitle = jobRoleTitle;
        StandardJobRoleId = standardJobRoleId;
        HierarchyLevelId = hierarchyLevelId;
    }
}