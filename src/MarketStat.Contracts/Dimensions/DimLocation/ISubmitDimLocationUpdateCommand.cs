namespace MarketStat.Contracts.Dimensions.DimLocation;

public interface ISubmitDimLocationUpdateCommand : ISubmitDimLocationCommand
{
    int LocationId { get; }
}