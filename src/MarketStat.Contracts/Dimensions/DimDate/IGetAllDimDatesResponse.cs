namespace MarketStat.Contracts.Dimensions.DimDate;

public interface IGetAllDimDatesResponse
{
    List<IGetDimDateResponse> Dates { get; }
}