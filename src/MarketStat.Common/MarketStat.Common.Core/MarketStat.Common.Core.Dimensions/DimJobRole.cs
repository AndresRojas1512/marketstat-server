namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimJobRole
{
    public int JobRoleId { get; set; }
    public string JobRoleTitle { get; set; }
    public string SeniorityLevel { get; set; }
    public int FieldId { get; set; }

    public DimJobRole(int jobRoleId, string jobRoleTitle, string seniorityLevel, int fieldId)
    {
        JobRoleId = jobRoleId;
        JobRoleTitle = jobRoleTitle;
        SeniorityLevel = seniorityLevel;
        FieldId = fieldId;
    }
}