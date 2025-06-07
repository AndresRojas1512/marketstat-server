using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_standard_job_role", Schema = "marketstat")]
public class DimStandardJobRoleDbModel
{
    [Key]
    [Column("standard_job_role_id")]
    public int StandardJobRoleId { get; set; }

    [Required]
    [Column("standard_job_role_code")]
    [StringLength(20)]
    public string StandardJobRoleCode { get; set; } = string.Empty;

    [Required]
    [Column("standard_job_role_title")]
    [StringLength(255)]
    public string StandardJobRoleTitle { get; set; } = string.Empty;
    
    [Required]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }
    
    [ForeignKey(nameof(IndustryFieldId))]
    public virtual DimIndustryFieldDbModel? DimIndustryField { get; set; }
    
    public virtual ICollection<DimJobRoleDbModel> DimJobRoles { get; set; }
    public virtual ICollection<DimStandardJobRoleHierarchyDbModel> DimStandardJobRoleHierarchies { get; set; }
    
    public DimStandardJobRoleDbModel()
    {
        DimJobRoles = new List<DimJobRoleDbModel>();
        DimStandardJobRoleHierarchies = new List<DimStandardJobRoleHierarchyDbModel>();
    }
}