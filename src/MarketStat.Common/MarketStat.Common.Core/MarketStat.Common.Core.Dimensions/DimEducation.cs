using MarketStat.Common.Enums;

namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEducation
{
    public int EducationId { get; set; }
    public string Specialization  { get; set; }
    public EducationLevel EducationLevel { get; set; }
    public int IndustryField { get; set; }

    public DimEducation(int educationId, string specialization, EducationLevel educationLevel, int industryField)
    {
        EducationId = educationId;
        Specialization = specialization;
        EducationLevel = educationLevel;
        IndustryField = industryField;
    }
}