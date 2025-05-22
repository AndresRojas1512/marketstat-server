using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_oblast")]
public class DimOblastDbModel
{
    [Key]
    [Column("oblast_id")]
    public int OblastId { get; set; }

    [Required]
    [Column("oblast_name")]
    [StringLength(255)]
    public string OblastName { get; set; } = string.Empty;
    
    [Required]
    [Column("district_id")]
    public int DistrictId { get; set; }
    
    [ForeignKey(nameof(DistrictId))]
    public virtual DimFederalDistrictDbModel? DimFederalDistrict { get; set; }

    public virtual ICollection<DimCityDbModel> DimCities { get; set; } = new List<DimCityDbModel>();

    public DimOblastDbModel() { DimCities = new List<DimCityDbModel>(); }
    
    public DimOblastDbModel(int oblastId, string oblastName, int districtId)
    {
        OblastId = oblastId;
        OblastName = oblastName;
        DistrictId = districtId;
        DimCities = new List<DimCityDbModel>();
    }
}