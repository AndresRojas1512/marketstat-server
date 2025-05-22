using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Database.Models;

[Table("dim_job_role")]
public class DimJobRoleDbModel
{
    [Key]
    [Column("job_role_id")]
    public int JobRoleId { get; set; }

    [Required]
    [Column("job_role_title")]
    [StringLength(255)]
    public string JobRoleTitle { get; set; } = string.Empty;
    
    [Required]
    [Column("standard_job_role_id")]
    public int StandardJobRoleId { get; set; }
    
    [ForeignKey(nameof(StandardJobRoleId))]
    public virtual DimStandardJobRoleDbModel? DimStandardJobRole { get; set; }
    
    [Required]
    [Column("hierarchy_level_id")]
    public int HierarchyLevelId { get; set; }
    
    [ForeignKey(nameof(HierarchyLevelId))]
    public virtual DimHierarchyLevelDbModel? DimHierarchyLevel { get; set; }
    
    public virtual ICollection<FactSalaryDbModel> FactSalaries { get; set; } = new List<FactSalaryDbModel>();
    
    public DimJobRoleDbModel() 
    {
        FactSalaries = new List<FactSalaryDbModel>();
    }

    public DimJobRoleDbModel(int jobRoleId, string jobRoleTitle, int standardJobRoleId, int hierarchyLevelId)
    {
        JobRoleId = jobRoleId;
        JobRoleTitle = jobRoleTitle;
        StandardJobRoleId = standardJobRoleId;
        HierarchyLevelId = hierarchyLevelId;
        FactSalaries = new List<FactSalaryDbModel>();
    }
}