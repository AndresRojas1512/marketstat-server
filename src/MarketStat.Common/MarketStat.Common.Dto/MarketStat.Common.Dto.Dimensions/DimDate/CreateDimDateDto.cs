using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;

public record CreateDimDateDto
{
    [Required]
    public DateOnly FullDate { get; init; }
}