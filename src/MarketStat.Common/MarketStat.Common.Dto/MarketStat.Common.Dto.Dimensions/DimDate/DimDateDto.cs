namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;

public record DimDateDto()
{
    int DateId { get; init; }
    DateOnly FullDate { get; init; }
    int Year { get; init; }
    int Quarter { get; init; }
    int Month { get; init; }
}