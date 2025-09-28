using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Tests.Common.Builders;

public class DimCityBuilder
{
    private int _cityId = 1;
    private string _cityName = "Default City";
    private int _oblastId = 1;

    public DimCityBuilder WithId(int id)
    {
        _cityId = id;
        return this;
    }

    public DimCityBuilder WithName(string name)
    {
        _cityName = name;
        return this;
    }

    public DimCityBuilder WithOblastId(int oblastId)
    {
        _oblastId = oblastId;
        return this;
    }

    public DimCity Build()
    {
        return new DimCity(_cityId, _cityName, _oblastId);
    }
}