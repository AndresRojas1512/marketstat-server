using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_standard_job_role_hierarchy")]
public class DimStandardJobRoleHierarchyDbModel
{
    [Key, Column("standard_job_role_id", Order = 0)]
    public int StandardJobRoleId { get; set; }
    
    [Key, Column("hierarchy_level_id", Order = 1)]
    public int HierarchyLevelId { get; set; }

    public DimStandardJobRoleHierarchyDbModel(int standardJobRoleId, int hierarchyLevelId)
    {
        StandardJobRoleId = standardJobRoleId;
        HierarchyLevelId = hierarchyLevelId;
    }
}