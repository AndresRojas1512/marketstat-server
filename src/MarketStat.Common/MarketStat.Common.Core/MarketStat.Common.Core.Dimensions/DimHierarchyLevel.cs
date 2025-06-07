namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimHierarchyLevel
{
    public int HierarchyLevelId { get; set; }
    
    public string HierarchyLevelCode { get; set; }
        
    public string HierarchyLevelName { get; set; }
    
    public virtual ICollection<DimStandardJobRoleHierarchy> DimStandardJobRoleHierarchies { get; set; }
    public virtual ICollection<DimJobRole> DimJobRoles { get; set; }
    
    public DimHierarchyLevel()
    {
        HierarchyLevelCode = string.Empty;
        HierarchyLevelName = string.Empty;

        DimStandardJobRoleHierarchies = new List<DimStandardJobRoleHierarchy>();
        DimJobRoles = new List<DimJobRole>();
    }
    
    public DimHierarchyLevel(int hierarchyLevelId, string hierarchyLevelCode, string hierarchyLevelName)
    {
        HierarchyLevelId = hierarchyLevelId;
        HierarchyLevelCode = hierarchyLevelCode ?? throw new ArgumentNullException(nameof(hierarchyLevelCode));
        HierarchyLevelName = hierarchyLevelName ?? throw new ArgumentNullException(nameof(hierarchyLevelName));
            
        DimStandardJobRoleHierarchies = new List<DimStandardJobRoleHierarchy>();
        DimJobRoles = new List<DimJobRole>();
    }
}