using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Tests.TestData.Builders.Dimensions;

public class DimLocationBuilder
{
    private int _locationId = 0;
    private string _cityName = "Moscow";
    private string _oblastName = "Moscow";
    private string _districtName = "Central Federal District";
    
    public DimLocationBuilder WithId(int id)
    {
        _locationId = id;
        return this;
    }
    
    public DimLocationBuilder WithCityName(string cityName)
    {
        _cityName = cityName;
        return this;
    }

    public DimLocationBuilder WithOblastName(string oblastName)
    {
        _oblastName = oblastName;
        return this;
    }
    
    public DimLocationBuilder WithDistrictName(string districtName)
    {
        _districtName = districtName;
        return this;
    }

    public DimLocation Build()
    {
        return new DimLocation(
            _locationId,
            _cityName,
            _oblastName,
            _districtName
        );
    }
}