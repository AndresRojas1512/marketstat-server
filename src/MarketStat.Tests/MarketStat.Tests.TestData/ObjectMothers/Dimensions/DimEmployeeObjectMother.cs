namespace MarketStat.Tests.TestData.ObjectMothers.Dimensions;

using MarketStat.Common.Core.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;

public static class DimEmployeeObjectMother
{
    public static DimEmployee ANewEmployee() =>
        new DimEmployeeBuilder()
            .WithId(0)
            .WithEmployeeRefId("new-ref-456")
            .Build();

    public static DimEmployee AnExistingEmployee() =>
        new DimEmployeeBuilder()
            .WithId(1)
            .WithEmployeeRefId("existing-ref-1")
            .Build();

    public static DimEmployee ASecondExistingEmployee() =>
        new DimEmployeeBuilder()
            .WithId(2)
            .WithEmployeeRefId("existing-ref-2")
            .Build();

    public static IEnumerable<DimEmployee> SomeEmployees()
    {
        return new List<DimEmployee>
        {
            AnExistingEmployee(),
            ASecondExistingEmployee(),
        };
    }
}
