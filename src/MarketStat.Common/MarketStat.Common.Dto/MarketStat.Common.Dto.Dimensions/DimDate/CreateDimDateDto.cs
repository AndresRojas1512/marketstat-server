namespace MarketStat.Common.Dto.Dimensions.DimDate;

using System.ComponentModel.DataAnnotations;

public record CreateDimDateDto
{
    [Required]
    public DateOnly FullDate { get; init; }
}
