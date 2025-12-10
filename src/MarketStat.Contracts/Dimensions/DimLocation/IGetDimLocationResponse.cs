namespace MarketStat.Contracts.Dimensions.DimLocation;

public interface IGetDimLocationResponse
{
    int LocationId { get; }
    string CityName { get; }
    string OblastName { get; }
    string DistrictName { get; }
}