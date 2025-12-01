namespace MarketStat.Tests.TestData.ObjectMothers.Dimensions;

using MarketStat.Common.Core.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;

public static class DimEducationObjectMother
{
    public static DimEducation ANewEducation() =>
        new DimEducationBuilder()
            .WithId(0)
            .WithSpecialtyName("Applied Mathematics")
            .WithSpecialtyCode("01.03.04")
            .Build();

    public static DimEducation AnExistingEducation() =>
        new DimEducationBuilder()
            .WithId(1)
            .WithSpecialtyName("Computer Science")
            .WithSpecialtyCode("09.03.01")
            .Build();

    public static DimEducation ASecondExistingEducation() =>
        new DimEducationBuilder()
            .WithId(2)
            .WithSpecialtyName("Economics")
            .WithSpecialtyCode("38.03.01")
            .Build();

    public static IEnumerable<DimEducation> SomeEducations()
    {
        return new List<DimEducation>
        {
            AnExistingEducation(),
            ASecondExistingEducation(),
        };
    }
}
