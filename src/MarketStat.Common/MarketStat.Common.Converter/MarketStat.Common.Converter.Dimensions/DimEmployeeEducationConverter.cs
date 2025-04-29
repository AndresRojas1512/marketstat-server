using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimEmployeeEducationConverter
{
    public static DimEmployeeEducationDbModel ToDbModel(DimEmployeeEducation domain)
    {
        return new DimEmployeeEducationDbModel(
            domain.EmployeeId,
            domain.EducationId,
            domain.GraduationYear
        );
    }

    public static DimEmployeeEducation ToDomain(DimEmployeeEducationDbModel dbModel)
    {
        return new DimEmployeeEducation(
            dbModel.EmployeeId,
            dbModel.EducationId,
            dbModel.GraduationYear
        );
    }
}