namespace MarketStat.Contracts.Dimensions.DimEmployer;

public interface IGetAllDimEmployersResponse
{
    List<IGetDimEmployerResponse> Employers { get; }
}