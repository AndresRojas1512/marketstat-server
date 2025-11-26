namespace MarketStat.Contracts.Dimensions.DimDate;

public interface IGetDimDateResponse
{
    int DateId { get; }
    DateOnly FullDate { get; }
    int Year { get; }
    int Quarter { get; }
    int Month { get; }
}