namespace MarketStat.Contracts.Dimensions.DimEmployee;

public interface IGetAllDimEmployeesResponse
{
    List<IGetDimEmployeeResponse> Employees { get; }
}