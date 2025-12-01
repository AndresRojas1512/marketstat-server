namespace MarketStat.Common.Dto.Dimensions.DimDate;

using System.ComponentModel.DataAnnotations;

public record UpdateDimDateDto
{
    [Required]
    public DateOnly FullDate { get; init; }
}
