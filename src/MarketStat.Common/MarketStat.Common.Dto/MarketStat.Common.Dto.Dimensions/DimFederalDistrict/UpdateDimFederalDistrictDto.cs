using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimFederalDistrict;

public record UpdateDimFederalDistrictDto
{
    [Required]
    [MaxLength(255)]
    public string DistrictName { get; init; } = default!;
}