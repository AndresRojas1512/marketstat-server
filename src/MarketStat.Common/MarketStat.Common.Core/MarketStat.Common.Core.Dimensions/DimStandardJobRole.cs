namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimStandardJobRole
{
    public int StandardJobRoleId { get; set; }
    public string StandardJobRoleTitle { get; set; }
    public int IndustryFieldId { get; set; }

    public DimStandardJobRole()
    {
        
    }

    public DimStandardJobRole(int standardJobRoleId, string standardJobRoleTitle, int industryFieldId)
    {
        StandardJobRoleId = standardJobRoleId;
        StandardJobRoleTitle = standardJobRoleTitle;
        IndustryFieldId = industryFieldId;
    }
}