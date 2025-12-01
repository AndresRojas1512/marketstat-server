namespace MarketStat.Tests.TestData.ObjectMothers.Dimensions;

using MarketStat.Common.Core.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;

public static class DimJobObjectMother
{
    public static DimJob ANewJob() =>
        new DimJobBuilder()
            .WithId(0)
            .WithStandardJobRoleTitle("QA Engineer")
            .WithIndustryFieldId(1)
            .Build();

    public static DimJob AnExistingJob() =>
        new DimJobBuilder()
            .WithId(1)
            .WithStandardJobRoleTitle("Software Engineer")
            .WithIndustryFieldId(1)
            .Build();

    public static DimJob ASecondExistingJob() =>
        new DimJobBuilder()
            .WithId(2)
            .WithStandardJobRoleTitle("Data Analyst")
            .WithIndustryFieldId(2)
            .Build();

    public static IEnumerable<DimJob> SomeJobs()
    {
        return new List<DimJob>
        {
            AnExistingJob(),
            ASecondExistingJob(),
        };
    }
}
