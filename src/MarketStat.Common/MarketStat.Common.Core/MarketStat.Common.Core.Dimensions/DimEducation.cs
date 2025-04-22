namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEducation
{
    public int EducationId { get; set; }
    public string Specialization  { get; set; }
    public string EducationLevel { get; set; }

    public DimEducation(int educationId, string specialization, string educationLevel)
    {
        EducationId = educationId;
        Specialization = specialization;
        EducationLevel = educationLevel;
    }
}