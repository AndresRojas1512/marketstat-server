using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimIndustryField;

public record UpdateDimIndustryFieldDto
{
    [Required]
    [MaxLength(255)]
    public string IndustryFieldName { get; init; } = default!;
}