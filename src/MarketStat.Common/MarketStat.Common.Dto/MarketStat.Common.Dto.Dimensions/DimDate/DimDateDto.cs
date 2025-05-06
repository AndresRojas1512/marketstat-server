namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Dimensions.DimDate;

public record DimDateDto(
    int DateId,
    DateOnly FullDate,
    int Year,
    int Quarter,
    int Month
);