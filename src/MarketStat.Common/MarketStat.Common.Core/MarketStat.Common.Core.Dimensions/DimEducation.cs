namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEducation
{
    public int EducationId { get; set; }
    public string SpecialtyName { get; set; }
    public string SpecialtyCode { get; set; }
    public int EducationLevelName { get; set; }

    public DimEducation()
    {
    }

    public DimEducation(int educationId, string specialtyName, string specialtyCode, int educationLevelName)
    {
        EducationId = educationId;
        SpecialtyName = specialtyName;
        SpecialtyCode = specialtyCode;
        EducationLevelName = educationLevelName;
    }
}