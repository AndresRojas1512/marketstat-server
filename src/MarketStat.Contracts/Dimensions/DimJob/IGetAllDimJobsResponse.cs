namespace MarketStat.Contracts.Dimensions.DimJob;

public interface IGetAllDimJobsResponse
{
    List<IGetDimJobResponse> Jobs { get; }
}