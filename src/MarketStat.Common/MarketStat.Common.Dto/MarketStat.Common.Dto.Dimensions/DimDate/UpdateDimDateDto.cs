using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;

public record UpdateDimDateDto
{
    [Required]
    public DateOnly FullDate { get; init; }
}