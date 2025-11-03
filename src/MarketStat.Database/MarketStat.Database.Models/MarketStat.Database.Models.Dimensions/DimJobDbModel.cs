using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Database.Models;

[Table("dim_job")]
public class DimJobDbModel
{
    [Key]
    [Column("job_id")]
    public int JobId { get; set; }
    
    [Required]
    [Column("job_role_title")]
    [StringLength(255)]
    public string JobRoleTitle { get; set; } = string.Empty;
    
    [Required]
    [Column("standard_job_role_title")]
    [StringLength(255)]
    public string StandardJobRoleTitle { get; set; } = string.Empty;
    
    [Required]
    [Column("hierarchy_level_name")]
    [StringLength(255)]
    public string HierarchyLevelName { get; set; } = string.Empty;

    [Required]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }
    
    public virtual DimIndustryFieldDbModel? IndustryField { get; set; }
    public virtual ICollection<FactSalaryDbModel> FactSalaries { get; set; } = new List<FactSalaryDbModel>();
}