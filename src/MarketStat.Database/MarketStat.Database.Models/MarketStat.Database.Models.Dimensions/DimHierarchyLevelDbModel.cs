using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_hierarchy_levels")]
public class DimHierarchyLevelDbModel
{
    [Key]
    [Column("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }
    
    [Required]
    [Column("hierarchy_level_name")]
    [StringLength(255)]
    public string HierarchyLevelName { get; set; }

    public DimHierarchyLevelDbModel(int hierarchyLevelId, string hierarchyLevelName)
    {
        HierarchyLevelId = hierarchyLevelId;
        HierarchyLevelName = hierarchyLevelName;
    }
}