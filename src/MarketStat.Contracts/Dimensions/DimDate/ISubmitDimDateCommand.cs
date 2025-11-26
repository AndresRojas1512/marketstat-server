namespace MarketStat.Contracts.Dimensions.DimDate;

public interface ISubmitDimDateCommand
{
    DateOnly FullDate { get; }
}