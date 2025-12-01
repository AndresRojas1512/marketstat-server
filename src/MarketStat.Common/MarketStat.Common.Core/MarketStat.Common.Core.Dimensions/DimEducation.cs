namespace MarketStat.Common.Core.Dimensions;

public class DimEducation
{
    public DimEducation()
    {
        SpecialtyName = string.Empty;
        SpecialtyCode = string.Empty;
        EducationLevelName = string.Empty;
    }

    public DimEducation(int educationId, string specialtyName, string specialtyCode, string educationLevelName)
    {
        EducationId = educationId;
        SpecialtyName = specialtyName;
        SpecialtyCode = specialtyCode;
        EducationLevelName = educationLevelName;
    }

    public int EducationId { get; set; }

    public string SpecialtyName { get; set; }

    public string SpecialtyCode { get; set; }

    public string EducationLevelName { get; set; }
}
