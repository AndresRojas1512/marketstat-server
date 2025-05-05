using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_federal_districts")]
public class DimFederalDistrictDbModel
{
    [Key]
    [Column("district_id")]
    public int DistrictId { get; set; }
    
    [Required]
    [Column("district_name")]
    [StringLength(255)]
    public string DistrictName { get; set; }

    public DimFederalDistrictDbModel(int districtId, string districtName)
    {
        DistrictId = districtId;
        DistrictName = districtName;
    }
}