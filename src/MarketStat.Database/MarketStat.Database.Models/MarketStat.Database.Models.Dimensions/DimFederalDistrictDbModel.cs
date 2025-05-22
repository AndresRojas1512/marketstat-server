using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketStat.Database.Models;

[Table("dim_federal_district")]
public class DimFederalDistrictDbModel
{
    [Key]
    [Column("district_id")]
    public int DistrictId { get; set; }
    
    [Required]
    [Column("district_name")]
    [StringLength(255)]
    public string DistrictName { get; set; } = string.Empty;
    
    public virtual ICollection<DimOblastDbModel> DimOblasts { get; set; } = new List<DimOblastDbModel>();

    public DimFederalDistrictDbModel() { DimOblasts = new List<DimOblastDbModel>(); }

    public DimFederalDistrictDbModel(int districtId, string districtName)
    {
        DistrictId = districtId;
        DistrictName = districtName;
        DimOblasts = new List<DimOblastDbModel>();
    }
}