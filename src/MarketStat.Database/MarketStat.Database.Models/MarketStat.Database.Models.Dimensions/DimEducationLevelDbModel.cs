using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_education_level")]
public class DimEducationLevelDbModel
{
    [Key]
    [Column("education_level_id")]
    public int EducationLevelId { get; set; }

    [Required]
    [Column("education_level_name")]
    [StringLength(255)]
    public string EducationLevelName { get; set; } = string.Empty;
    
    public virtual ICollection<DimEducationDbModel> DimEducations { get; set; }
    
    public DimEducationLevelDbModel() 
    {
        DimEducations = new List<DimEducationDbModel>();
    }

    public DimEducationLevelDbModel(int educationLevelId, string educationLevelName)
    {
        EducationLevelId = educationLevelId;
        EducationLevelName = educationLevelName;
    }
}