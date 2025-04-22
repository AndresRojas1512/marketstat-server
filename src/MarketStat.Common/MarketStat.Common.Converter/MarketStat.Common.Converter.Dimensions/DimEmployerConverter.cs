using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimEmployerConverter
{
    public static DimEmployerDbModel ToDbModel(DimEmployer dimEmployer)
    {
        return new DimEmployerDbModel(
            employerId: dimEmployer.EmployerId,
            employerName: dimEmployer.EmployerName,
            industry: dimEmployer.Industry,
            isPublic: dimEmployer.IsPublic
        );
    }

    public static DimEmployer ToDomain(DimEmployerDbModel dbDimEmployer)
    {
        return new DimEmployer(
            dbDimEmployer.EmployerId,
            dbDimEmployer.EmployerName,
            dbDimEmployer.Industry,
            dbDimEmployer.IsPublic
        );
    }
}