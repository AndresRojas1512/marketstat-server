using System.ComponentModel.DataAnnotations;

namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;

public record CreateDimDateDto
{
    [Required]
    public DateOnly FullDate { get; init; }
    
    [Required]
    public int Year { get; init; }
    
    [Required]
    [Range(1, 4)]
    public int Quarter { get; init; }
    
    [Required]
    [Range(1, 12)]
    public int Month { get; init; }
}