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
    [Column("specialty")]
    [StringLength(255)]
    public string Specialty { get; set; }
    
    [Required]
    [Column("specialty_code")]
    [StringLength(255)]
    public string SpecialtyCode { get; set; }
    
    [Required]
    [Column("education_level_id")]
    public int EducationLevelId { get; set; }
    
    [Required]
    [Column("industry_field_id")]
    public int IndustryFieldId { get; set; }

    public DimEducationDbModel(int educationId, string specialty, string specialtyCode, int educationLevelId, int industryFieldId)
    {
        EducationId = educationId;
        Specialty = specialty;
        SpecialtyCode = specialtyCode;
        EducationLevelId = educationLevelId;
        IndustryFieldId = industryFieldId;
    }
}