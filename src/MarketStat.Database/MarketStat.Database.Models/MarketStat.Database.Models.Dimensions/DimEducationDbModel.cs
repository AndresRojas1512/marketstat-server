using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_education_level")]
public class DimEducationDbModel
{
    [Key]
    [Column("education_id")]
    public int EducationId { get; set; }
    
    [Required]
    [Column("specialization")]
    [StringLength(255)]
    public string Specialization { get; set; }
    
    [Required]
    [Column("education_level")]
    public int EducationLevelId { get; set; }
    
    [Required]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }

    public DimEducationDbModel(int educationId, string specialization, int educationLevelId, int industryFieldId)
    {
        EducationId = educationId;
        Specialization = specialization;
        EducationLevelId = educationLevelId;
        IndustryFieldId = industryFieldId;
    }
}