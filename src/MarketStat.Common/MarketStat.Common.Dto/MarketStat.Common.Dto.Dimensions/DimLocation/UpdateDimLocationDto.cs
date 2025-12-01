namespace MarketStat.Common.Dto.Dimensions.DimLocation;

using System.ComponentModel.DataAnnotations;

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
