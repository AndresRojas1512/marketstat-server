namespace MarketStat.Tests.TestData.ObjectMothers.Dimensions;

using MarketStat.Common.Core.Dimensions;
using MarketStat.Tests.TestData.Builders.Dimensions;

public static class DimIndustryFieldObjectMother
{
    public static DimIndustryField ANewIndustryField() =>
        new DimIndustryFieldBuilder()
            .WithId(0)
            .WithIndustryFieldCode("B.02")
            .WithIndustryFieldName("Finance")
            .Build();

    public static DimIndustryField AnExistingIndustryField() =>
        new DimIndustryFieldBuilder()
            .WithId(1)
            .WithIndustryFieldCode("A.01")
            .WithIndustryFieldName("Information Technology")
            .Build();

    public static DimIndustryField ASecondExistingIndustryField() =>
        new DimIndustryFieldBuilder()
            .WithId(2)
            .WithIndustryFieldCode("C.03")
            .WithIndustryFieldName("Healthcare")
            .Build();

    public static IEnumerable<DimIndustryField> SomeIndustryFields()
    {
        return new List<DimIndustryField>
        {
            AnExistingIndustryField(),
            ASecondExistingIndustryField(),
        };
    }
}
