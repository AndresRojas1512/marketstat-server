namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimJob
{
    public int JobId { get; set; }
    public string JobRoleTitle { get; set; }
    public string StandardJobRoleTitle { get; set; }
    public string HierarchyLevelName { get; set; }
    public int IndustryFieldId { get; set; }

    public DimJob()
    {
    }

    public DimJob(int jobId, string jobRoleTitle, string standardJobRoleTitle, string hierarchyLevelName,
        int industryFieldId)
    {
        JobId = jobId;
        JobRoleTitle = jobRoleTitle;
        StandardJobRoleTitle = standardJobRoleTitle;
        HierarchyLevelName = hierarchyLevelName;
        IndustryFieldId = industryFieldId;
    }
}