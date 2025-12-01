namespace MarketStat.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("dim_education")]
public class DimEducationDbModel
{
    [Key]
    [Column("education_id")]
    public int EducationId { get; set; }

    [Required]
    [Column("specialty_name")]
    [StringLength(255)]
    public string SpecialtyName { get; set; } = string.Empty;

    [Required]
    [Column("specialty_code")]
    [StringLength(255)]
    public string SpecialtyCode { get; set; } = string.Empty;

    [Required]
    [Column("education_level_name")]
    [StringLength(255)]
    public string EducationLevelName { get; set; } = string.Empty;

    public virtual ICollection<DimEmployeeDbModel> DimEmployees { get; } = new List<DimEmployeeDbModel>();
}
