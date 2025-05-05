using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimEducationLevelConverter
{
    public static DimEducationLevelDbModel ToDbModel(DimEducationLevel educationLevel)
    {
        return new DimEducationLevelDbModel(
            educationLevel.EducationLevelId,
            educationLevel.EducationLevelName
        );
    }

    public static DimEducationLevel ToDomain(DimEducationLevelDbModel dbEducationLevel)
    {
        return new DimEducationLevel(
            dbEducationLevel.EducationLevelId,
            dbEducationLevel.EducationLevelName
        );
    }
}