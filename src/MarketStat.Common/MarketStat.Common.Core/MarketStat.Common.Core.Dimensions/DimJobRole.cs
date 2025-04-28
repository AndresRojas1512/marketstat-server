namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimJobRole
{
    public int JobRoleId { get; set; }
    public string JobRoleTitle { get; set; }
    public int IndustryFieldId { get; set; }
    public int HierarchyLevelId { get; set; }
    public DimJobRole(int jobRoleId, string jobRoleTitle, int industryFieldId, int hierarchyLevelId)
    {
        JobRoleId = jobRoleId;
        JobRoleTitle = jobRoleTitle;
        IndustryFieldId = industryFieldId;
        HierarchyLevelId = hierarchyLevelId;
    }
}