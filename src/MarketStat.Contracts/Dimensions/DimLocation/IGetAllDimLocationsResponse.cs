namespace MarketStat.Contracts.Dimensions.DimLocation;

public interface IGetAllDimLocationsResponse
{
    List<IGetDimLocationResponse> Locations { get; }
}