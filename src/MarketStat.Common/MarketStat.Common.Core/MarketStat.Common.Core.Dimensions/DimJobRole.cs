namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimJobRole
{
    public int JobRoleId { get; set; }
    public string JobRoleTitle { get; set; }
    public string SeniorityLevel { get; set; }
    public int IndustryFieldId { get; set; }

    public DimJobRole(int jobRoleId, string jobRoleTitle, string seniorityLevel, int industryFieldId)
    {
        JobRoleId = jobRoleId;
        JobRoleTitle = jobRoleTitle;
        SeniorityLevel = seniorityLevel;
        IndustryFieldId = industryFieldId;
    }
}