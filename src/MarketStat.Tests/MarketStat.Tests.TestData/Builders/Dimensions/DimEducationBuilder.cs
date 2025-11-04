using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Tests.TestData.Builders.Dimensions;

public class DimEducationBuilder
{
    private int _educationId = 0;
    private string _specialtyName = "Computer Science";
    private string _specialtyCode = "09.03.01";
    private string _educationLevelName = "Bachelor's Degree";

    public DimEducationBuilder WithId(int id)
    {
        _educationId = id;
        return this;
    }

    public DimEducationBuilder WithSpecialtyName(string name)
    {
        _specialtyName = name;
        return this;
    }

    public DimEducationBuilder WithSpecialtyCode(string code)
    {
        _specialtyCode = code;
        return this;
    }
    
    public DimEducationBuilder WithEducationLevelName(string levelName)
    {
        _educationLevelName = levelName;
        return this;
    }
    
    public DimEducation Build()
    {
        return new DimEducation(
            _educationId,
            _specialtyName,
            _specialtyCode,
            _educationLevelName
        );
    }
}