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
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }
    
    [Required]
    [Column("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }

    public DimJobRoleDbModel(int jobRoleId, string jobRoleTitle, int industryFieldId, int hierarchyLevelId)
    {
        JobRoleId = jobRoleId;
        JobRoleTitle = jobRoleTitle;
        IndustryFieldId = industryFieldId;
        HierarchyLevelId = hierarchyLevelId;
    }
}