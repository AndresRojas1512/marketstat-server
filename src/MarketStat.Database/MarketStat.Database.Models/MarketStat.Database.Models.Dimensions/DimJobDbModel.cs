using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_job")]
public class DimJobDbModel
{
    [Key]
    [Column("job_id")]
    public int JobId { get; set; }
    
    [Required]
    [Column("job_role_title")]
    public string JobRoleTitle { get; set; } = string.Empty;
    
    [Required]
    [Column("standard_job_role_title")]
    public string StandardJobRoleTitle { get; set; } = string.Empty;
    
    [Required]
    [Column("hierarchy_level_name")]
    public string HierarchyLevelName { get; set; } = string.Empty;

    [Required]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }

    public DimJobDbModel()
    {
    }

    public DimJobDbModel(int jobId, string jobRoleTitle, string standardJobRoleTitle, string hierarchyLevelName,
        int industryFieldId)
    {
        JobId = jobId;
        JobRoleTitle = jobRoleTitle;
        StandardJobRoleTitle = standardJobRoleTitle;
        HierarchyLevelName = hierarchyLevelName;
        IndustryFieldId = industryFieldId;
    }
}