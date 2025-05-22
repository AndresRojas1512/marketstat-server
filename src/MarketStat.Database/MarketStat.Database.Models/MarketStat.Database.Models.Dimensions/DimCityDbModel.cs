using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Database.Models;

[Table("dim_city")]
public class DimCityDbModel
{
    [Key]
    [Column("city_id")]
    public int CityId { get; set; }

    [Required]
    [Column("city_name")]
    [StringLength(255)]
    public string CityName { get; set; } = string.Empty;
    
    [Required]
    [Column("oblast_id")]
    public int OblastId { get; set; }
    
    [ForeignKey(nameof(OblastId))]
    public virtual DimOblastDbModel? DimOblast { get; set; }
    
    public virtual ICollection<FactSalaryDbModel> FactSalaries { get; set; } = new List<FactSalaryDbModel>();

    public DimCityDbModel()
    {
        FactSalaries = new List<FactSalaryDbModel>();
    }

    public DimCityDbModel(int cityId, string cityName, int oblastId)
    {
        CityId = cityId;
        CityName = cityName;
        OblastId = oblastId;
        FactSalaries = new List<FactSalaryDbModel>();
    }
}