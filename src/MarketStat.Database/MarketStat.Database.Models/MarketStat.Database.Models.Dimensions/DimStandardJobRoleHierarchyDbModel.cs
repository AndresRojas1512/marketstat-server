using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_standard_job_role_hierarchy")]
public class DimStandardJobRoleHierarchyDbModel
{
    [Column("standard_job_role_id")]
    public int StandardJobRoleId { get; set; }
    
    [Column("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }
    
    [ForeignKey(nameof(StandardJobRoleId))]
    public virtual DimStandardJobRoleDbModel? StandardJobRole { get; set; }
    
    [ForeignKey(nameof(HierarchyLevelId))]
    public virtual DimHierarchyLevelDbModel? HierarchyLevel { get; set; }
    
    public DimStandardJobRoleHierarchyDbModel() { }

    public DimStandardJobRoleHierarchyDbModel(int standardJobRoleId, int hierarchyLevelId)
    {
        StandardJobRoleId = standardJobRoleId;
        HierarchyLevelId = hierarchyLevelId;
    }
}