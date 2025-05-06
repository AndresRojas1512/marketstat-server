using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimOblast;

public class CreateDimOblastDto
{
    [Required]
    [MaxLength(255)]
    string OblastName { get; init; } = default!;
    
    [Required]
    public int DistrictId { get; init; }
}