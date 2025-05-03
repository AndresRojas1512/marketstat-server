using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_standard_job_roles")]
public class DimStandardJobRoleDbModel
{
    [Key]
    [Column("standard_job_role_id")]
    public int StandardJobRoleId { get; set; }
    
    [Required]
    [Column("standard_job_role_title")]
    [StringLength(255)]
    public string StandardJobRoleTitle { get; set; }
    
    [Required]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }

    public DimStandardJobRoleDbModel(int standardJobRoleId, string standardJobRoleTitle, int industryFieldId)
    {
        StandardJobRoleId = standardJobRoleId;
        StandardJobRoleTitle = standardJobRoleTitle;
        IndustryFieldId = industryFieldId;
    }
}