using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Services.Tests.TestData.Builders;

namespace MarketStat.Services.Tests.TestData.ObjectMothers;

public class DimEmployerObjectMother
{
    public static DimEmployer AValidNewEmployer() =>
        new DimEmployerBuilder()
            .WithId(0)
            .WithName("OOO New Employer")
            .WithInn("1234567890")
            .Build();
    
    public static DimEmployer AnExistingEmployer() =>
        new DimEmployerBuilder()
            .WithId(1)
            .WithName("OOO Old Employer")
            .WithInn("9876543210")
            .Build();
    
    public static DimEmployer ASecondExistingEmployer() =>
        new DimEmployerBuilder()
            .WithId(2)
            .WithName("OOO Random Employer")
            .WithInn("1111222233")
            .Build();
    
    public static DimEmployer AnEmployerWithInvalidName() =>
        new DimEmployerBuilder()
            .WithName(null!)
            .Build();
    
    public static IEnumerable<DimEmployer> SomeEmployers()
    {
        return new List<DimEmployer>
        {
            AnExistingEmployer(),
            ASecondExistingEmployer()
        };
    }
}