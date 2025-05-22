using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_education")]
public class DimEducationDbModel
{
    [Key]
    [Column("education_id")]
    public int EducationId { get; set; }

    [Required]
    [Column("specialty")]
    [StringLength(255)]
    public string Specialty { get; set; } = string.Empty;

    [Required]
    [Column("specialty_code")]
    [StringLength(255)]
    public string SpecialtyCode { get; set; } = string.Empty;
    
    [Required]
    [Column("education_level_id")]
    public int EducationLevelId { get; set; }
    
    [ForeignKey(nameof(EducationLevelId))]
    public virtual DimEducationLevelDbModel? DimEducationLevel { get; set; }
    
    public virtual ICollection<DimEmployeeEducationDbModel> DimEmployeeEducations { get; set; }

    public DimEducationDbModel() 
    {
        DimEmployeeEducations = new List<DimEmployeeEducationDbModel>();
    }
    
    public DimEducationDbModel(int educationId, string specialty, string specialtyCode, int educationLevelId)
    {
        EducationId = educationId;
        Specialty = specialty;
        SpecialtyCode = specialtyCode;
        EducationLevelId = educationLevelId;
    }
}