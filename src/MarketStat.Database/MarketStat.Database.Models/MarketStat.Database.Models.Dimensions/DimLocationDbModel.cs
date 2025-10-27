using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_location")]
public class DimLocationDbModel
{
    [Key]
    [Column("location_id")]
    public int LocationId { get; set; }
    
    [Required]
    [Column("city_name")]
    [StringLength(255)]
    public string CityName { get; set; } = string.Empty;
    
    [Required]
    [Column("oblast_name")]
    [StringLength(255)]
    public string OblastName { get; set; } = string.Empty;
    
    [Required]
    [Column("district_name")]
    [StringLength(255)]
    public string DistrictName { get; set; } = string.Empty;
}