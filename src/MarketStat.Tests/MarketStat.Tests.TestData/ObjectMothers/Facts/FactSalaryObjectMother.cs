using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Tests.TestData.Builders.Facts;

namespace MarketStat.Tests.TestData.ObjectMothers.Facts;

public static class FactSalaryObjectMother
{
    public static FactSalary ANewSalary() =>
        new FactSalaryBuilder()
            .WithId(0)
            .WithSalaryAmount(120000)
            .Build();
    
    public static FactSalary AnExistingSalary() =>
        new FactSalaryBuilder()
            .WithId(1L)
            .WithSalaryAmount(100000)
            .Build();
    
    public static FactSalary ASecondExistingSalary() =>
        new FactSalaryBuilder()
            .WithId(2L)
            .WithSalaryAmount(250000)
            .Build();
    
    public static IEnumerable<FactSalary> SomeSalaries()
    {
        return new List<FactSalary>
        {
            AnExistingSalary(),
            ASecondExistingSalary()
        };
    }
}