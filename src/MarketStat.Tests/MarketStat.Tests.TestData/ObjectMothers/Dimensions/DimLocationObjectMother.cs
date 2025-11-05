using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;

namespace MarketStat.Tests.TestData.ObjectMothers.Dimensions;

public static class DimLocationObjectMother
{
    public static DimLocation ANewLocation() =>
        new DimLocationBuilder()
            .WithId(0)
            .WithCityName("Yekaterinburg")
            .WithOblastName("Sverdlovsk Oblast")
            .WithDistrictName("Ural Federal District")
            .Build();
    
    public static DimLocation AnExistingLocation() =>
        new DimLocationBuilder()
            .WithId(1)
            .WithCityName("Moscow")
            .WithOblastName("Moscow")
            .WithDistrictName("Central Federal District")
            .Build();
    
    public static DimLocation ASecondExistingLocation() =>
        new DimLocationBuilder()
            .WithId(2)
            .WithCityName("Saint Petersburg")
            .WithOblastName("Saint Petersburg")
            .WithDistrictName("Northwestern Federal District")
            .Build();
    
    public static IEnumerable<DimLocation> SomeLocations()
    {
        return new List<DimLocation>
        {
            AnExistingLocation(),
            ASecondExistingLocation()
        };
    }
}