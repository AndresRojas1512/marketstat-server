using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_job_roles")]
public class DimJobRoleDbModel
{
    [Key]
    [Column("job_role_id")]
    public int JobRoleId { get; set; }
    
    [Required]
    [Column("job_role_title")]
    [StringLength(255)]
    public string JobRoleTitle { get; set; }
    
    [Required]
    [Column("standard_job_role_id")]
    public int StandardJobRoleId { get; set; }
    
    [Required]
    [Column("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }

    public DimJobRoleDbModel(int jobRoleId, string jobRoleTitle, int standardJobRoleId, int hierarchyLevelId)
    {
        JobRoleId = jobRoleId;
        JobRoleTitle = jobRoleTitle;
        StandardJobRoleId = standardJobRoleId;
        HierarchyLevelId = hierarchyLevelId;
    }
}