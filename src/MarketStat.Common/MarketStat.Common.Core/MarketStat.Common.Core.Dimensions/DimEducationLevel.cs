namespace MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

public class DimEducationLevel
{
    public int EducationLevelId { get; set; }
    public string EducationLevelName { get; set; }

    public DimEducationLevel(int educationLevelId, string educationLevelName)
    {
        EducationLevelId = educationLevelId;
        EducationLevelName = educationLevelName;
    }
}