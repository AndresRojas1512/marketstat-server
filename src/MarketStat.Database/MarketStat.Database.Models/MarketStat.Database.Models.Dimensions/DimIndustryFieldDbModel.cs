using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_industry_fields")]
public class DimIndustryFieldDbModel
{
    [Key]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }
    
    [Required]
    [Column("industry_field_name")]
    [StringLength(255)]
    public string IndustryFieldName { get; set; }

    public DimIndustryFieldDbModel(int industryFieldId, string industryFieldName)
    {
        IndustryFieldId = industryFieldId;
        IndustryFieldName = industryFieldName;
    }
}