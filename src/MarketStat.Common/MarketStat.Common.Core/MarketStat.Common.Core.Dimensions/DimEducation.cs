namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEducation
{
    public int EducationId { get; set; }
    public string Specialization  { get; set; }
    public int EducationLevelId { get; set; }
    public int IndustryFieldId { get; set; }

    public DimEducation(int educationId, string specialization, int educationLevelId, int industryFieldId)
    {
        EducationId = educationId;
        Specialization = specialization;
        EducationLevelId = educationLevelId;
        IndustryFieldId = industryFieldId;
    }
}