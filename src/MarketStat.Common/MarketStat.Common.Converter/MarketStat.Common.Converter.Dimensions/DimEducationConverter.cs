using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimEducationConverter
{
    public static DimEducationDbModel ToDbModel(DimEducation dimEducation)
    {
        return new DimEducationDbModel(
            dimEducation.EducationId,
            dimEducation.Specialty,
            dimEducation.SpecialtyCode,
            dimEducation.EducationLevelId
        );
    }

    public static DimEducation ToDomain(DimEducationDbModel dbEducation)
    {
        return new DimEducation(
            dbEducation.EducationId,
            dbEducation.Specialty,
            dbEducation.SpecialtyCode,
            dbEducation.EducationLevelId
        );
    }
}