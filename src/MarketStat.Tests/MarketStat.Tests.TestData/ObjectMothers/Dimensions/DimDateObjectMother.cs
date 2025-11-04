using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;

namespace MarketStat.Tests.TestData.ObjectMothers.Dimensions;

public static class DimDateObjectMother
{
    public static DimDate ANewDate() =>
        new DimDateBuilder()
            .WithId(0)
            .WithFullDate(2025, 10, 31)
            .Build();
    
    public static DimDate AnExistingDate() =>
        new DimDateBuilder()
            .WithId(1)
            .WithFullDate(2024, 1, 1)
            .Build();
    
    public static DimDate ASecondExistingDate() =>
        new DimDateBuilder()
            .WithId(2)
            .WithFullDate(2024, 4, 1)
            .Build();
    
    public static IEnumerable<DimDate> SomeDates()
    {
        return new List<DimDate>
        {
            AnExistingDate(),
            ASecondExistingDate()
        };
    }
}