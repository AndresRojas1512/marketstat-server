using MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.Dimensions;

public static class DimEducationConverter
{
    public static DimEducationDbModel ToDbModel(DimEducation dimEducation)
    {
        ArgumentNullException.ThrowIfNull(dimEducation);

        return new DimEducationDbModel
        {
            EducationId = dimEducation.EducationId,
            SpecialtyName = dimEducation.SpecialtyName,
            SpecialtyCode = dimEducation.SpecialtyCode,
            EducationLevelName = dimEducation.EducationLevelName,
        };
    }

    public static DimEducation ToDomain(DimEducationDbModel dbEducation)
    {
        ArgumentNullException.ThrowIfNull(dbEducation);

        return new DimEducation(
            dbEducation.EducationId,
            dbEducation.SpecialtyName,
            dbEducation.SpecialtyCode,
            dbEducation.EducationLevelName);
    }
}
