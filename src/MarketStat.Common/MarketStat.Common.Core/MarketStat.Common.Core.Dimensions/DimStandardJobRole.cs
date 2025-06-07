namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimStandardJobRole
{
    public int StandardJobRoleId { get; set; }

    public string StandardJobRoleCode { get; set; }

    public string StandardJobRoleTitle { get; set; }
        
    public int IndustryFieldId { get; set; }

    public virtual DimIndustryField? DimIndustryField { get; set; }
    public virtual ICollection<DimJobRole> DimJobRoles { get; set; }
    public virtual ICollection<DimStandardJobRoleHierarchy> DimStandardJobRoleHierarchies { get; set; }

    
    public DimStandardJobRole()
    {
        StandardJobRoleCode = string.Empty;
        StandardJobRoleTitle = string.Empty;

        DimJobRoles = new List<DimJobRole>();
        DimStandardJobRoleHierarchies = new List<DimStandardJobRoleHierarchy>();
    }
    
    public DimStandardJobRole(int standardJobRoleId, string standardJobRoleCode, string standardJobRoleTitle, int industryFieldId)
    {
        StandardJobRoleId = standardJobRoleId;
        StandardJobRoleCode = standardJobRoleCode ?? throw new ArgumentNullException(nameof(standardJobRoleCode));
        StandardJobRoleTitle = standardJobRoleTitle ?? throw new ArgumentNullException(nameof(standardJobRoleTitle));
        IndustryFieldId = industryFieldId;

        DimJobRoles = new List<DimJobRole>();
        DimStandardJobRoleHierarchies = new List<DimStandardJobRoleHierarchy>();
    }
}