using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public class DimEmployeeConverter
{
    public static DimEmployeeDbModel ToDbModel(DimEmployee dimEmployee)
    {
        return new DimEmployeeDbModel(
            dimEmployee.EmployeeId,
            dimEmployee.BirthDate,
            dimEmployee.CareerStartDate
        );
    }

    public static DimEmployee ToDomain(DimEmployeeDbModel dbEmployee)
    {
        return new DimEmployee(
            dbEmployee.EmployeeId,
            dbEmployee.BirthDate,
            dbEmployee.CareerStartDate
        );
    }
}