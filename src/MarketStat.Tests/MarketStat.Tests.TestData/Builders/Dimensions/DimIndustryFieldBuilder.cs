namespace MarketStat.Tests.TestData.Builders.Dimensions;

using MarketStat.Common.Core.Dimensions;

public class DimIndustryFieldBuilder
{
    private int _industryFieldId;
    private string _industryFieldCode = "A.01";
    private string _industryFieldName = "Information Technology";

    public DimIndustryFieldBuilder WithId(int id)
    {
        _industryFieldId = id;
        return this;
    }

    public DimIndustryFieldBuilder WithIndustryFieldCode(string code)
    {
        _industryFieldCode = code;
        return this;
    }

    public DimIndustryFieldBuilder WithIndustryFieldName(string name)
    {
        _industryFieldName = name;
        return this;
    }

    public DimIndustryField Build()
    {
        return new DimIndustryField(
            _industryFieldId,
            _industryFieldCode,
            _industryFieldName);
    }
}
