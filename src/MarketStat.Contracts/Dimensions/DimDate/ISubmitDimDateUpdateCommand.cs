namespace MarketStat.Contracts.Dimensions.DimDate;

public interface ISubmitDimDateUpdateCommand : ISubmitDimDateCommand
{
    int DateId { get; }
}