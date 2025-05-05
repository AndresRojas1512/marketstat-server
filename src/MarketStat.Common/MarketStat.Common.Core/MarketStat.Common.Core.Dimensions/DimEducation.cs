namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEducation
{
    public int EducationId { get; set; }
    public string Specialty  { get; set; }
    public string SpecialtyCode { get; set; }
    public int EducationLevelId { get; set; }
    public int IndustryFieldId { get; set; }

    public DimEducation(int educationId, string specialty, string specialtyCode, int educationLevelId, int industryFieldId)
    {
        EducationId = educationId;
        Specialty = specialty;
        SpecialtyCode = specialtyCode;
        EducationLevelId = educationLevelId;
        IndustryFieldId = industryFieldId;
    }
}