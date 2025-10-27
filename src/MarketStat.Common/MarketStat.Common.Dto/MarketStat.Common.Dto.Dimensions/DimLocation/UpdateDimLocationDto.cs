using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimLocation;

public class UpdateDimLocationDto
{
    [Required]
    [StringLength(255)]
    public string CityName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string OblastName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string DistrictName { get; set; } = string.Empty;
}