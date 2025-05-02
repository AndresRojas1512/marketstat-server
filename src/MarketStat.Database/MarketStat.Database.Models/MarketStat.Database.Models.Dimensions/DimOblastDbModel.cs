using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_oblasts")]
public class DimOblastDbModel
{
    [Key]
    [Column("oblast_id")]
    public int OblastId { get; set; }
    
    [Required]
    [Column("oblast_name")]
    [StringLength(255)]
    public string OblastName { get; set; }
    
    [Required]
    [Column("district_id")]
    public int DistrictId { get; set; }

    public DimOblastDbModel(int oblastId, string oblastName, int districtId)
    {
        OblastId = oblastId;
        OblastName = oblastName;
        DistrictId = districtId;
    }
}