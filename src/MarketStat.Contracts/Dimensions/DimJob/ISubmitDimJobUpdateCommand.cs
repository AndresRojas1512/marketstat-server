namespace MarketStat.Contracts.Dimensions.DimJob;

public interface ISubmitDimJobUpdateCommand : ISubmitDimJobCommand
{
    int JobId { get; }
}