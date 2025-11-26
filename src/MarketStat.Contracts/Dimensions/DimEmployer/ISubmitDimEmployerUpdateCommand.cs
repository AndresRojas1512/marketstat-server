namespace MarketStat.Contracts.Dimensions.DimEmployer;

public interface ISubmitDimEmployerUpdateCommand : ISubmitDimEmployerCommand
{
    int EmployerId { get; }
}