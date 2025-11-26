namespace MarketStat.Contracts.Dimensions.DimEmployee;

public interface ISubmitDimEmployeeUpdateCommand : ISubmitDimEmployeeCommand
{
    int EmployeeId { get; }
}