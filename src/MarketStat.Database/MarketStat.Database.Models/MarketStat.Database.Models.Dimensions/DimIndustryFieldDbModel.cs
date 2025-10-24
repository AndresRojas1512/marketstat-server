using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_industry_field")]
public class DimIndustryFieldDbModel
{
    [Key]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }

    [Required]
    [Column("industry_field_code")]
    [StringLength(10)]
    public string IndustryFieldCode { get; set; } = string.Empty;

    [Required]
    [Column("industry_field_name")]
    [StringLength(255)]
    public string IndustryFieldName { get; set; } = string.Empty;
    
    public virtual ICollection<DimEmployerDbModel> DimEmployers { get; set; }
    public virtual ICollection<DimJobDbModel> DimJobs { get; set; }
    
    public DimIndustryFieldDbModel() 
    {
        DimEmployers = new List<DimEmployerDbModel>();
        DimJobs = new List<DimJobDbModel>();
    }
}