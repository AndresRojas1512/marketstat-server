using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimCity;

public record UpdateDimCityDto
{
    [Required]
    [MaxLength(255)]
    public string CityName { get; init; } = default!;
    
    [Required]
    public int OblastId { get; init; }
}