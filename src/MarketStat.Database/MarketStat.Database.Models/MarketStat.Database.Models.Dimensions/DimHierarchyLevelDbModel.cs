using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_hierarchy_level")]
public class DimHierarchyLevelDbModel
{
    [Key]
    [Column("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }

    [Required]
    [Column("hierarchy_level_code")]
    [StringLength(10)]
    public string HierarchyLevelCode { get; set; } = string.Empty;

    [Required]
    [Column("hierarchy_level_name")]
    [StringLength(255)]
    public string HierarchyLevelName { get; set; } = string.Empty;

    public virtual ICollection<DimStandardJobRoleHierarchyDbModel> DimStandardJobRoleHierarchies { get; set; }
    public virtual ICollection<DimJobRoleDbModel> DimJobRoles { get; set; }

    public DimHierarchyLevelDbModel() 
    {
        DimStandardJobRoleHierarchies = new List<DimStandardJobRoleHierarchyDbModel>();
        DimJobRoles = new List<DimJobRoleDbModel>();
    }
}