namespace MarketStat.Contracts.Dimensions.DimLocation;

public interface ISubmitDimLocationCommand
{
    string CityName { get; }
    string OblastName { get; }
    string DistrictName { get; }
}