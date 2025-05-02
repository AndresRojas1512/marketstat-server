using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_cities")]
public class DimCityDbModel
{
    [Key]
    [Column("city_id")]
    public int CityId { get; set; }
    
    [Required]
    [Column("city_name")]
    [StringLength(255)]
    public string CityName { get; set; }
    
    [Required]
    [Column("oblast_id")]
    public int OblastId { get; set; }

    public DimCityDbModel(int cityId, string cityName, int oblastId)
    {
        CityId = cityId;
        CityName = cityName;
        OblastId = oblastId;
    }
}