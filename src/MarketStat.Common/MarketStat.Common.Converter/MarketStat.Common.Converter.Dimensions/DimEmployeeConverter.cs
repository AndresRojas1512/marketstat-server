using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimEmployeeConverter
{
    public static DimEmployeeDbModel ToDbModel(DimEmployee domainEmployee)
    {
        if (domainEmployee == null)
            throw new ArgumentNullException(nameof(domainEmployee));

        return new DimEmployeeDbModel
        {
            EmployeeId = domainEmployee.EmployeeId,
            EmployeeRefId = domainEmployee.EmployeeRefId,
            BirthDate = domainEmployee.BirthDate,
            CareerStartDate = domainEmployee.CareerStartDate,
            Gender = domainEmployee.Gender
        };
    }
    
    public static DimEmployee ToDomain(DimEmployeeDbModel dbEmployee)
    {
        if (dbEmployee == null)
            throw new ArgumentNullException(nameof(dbEmployee));

        return new DimEmployee
        {
            EmployeeId = dbEmployee.EmployeeId,
            EmployeeRefId = dbEmployee.EmployeeRefId,
            BirthDate = dbEmployee.BirthDate,
            CareerStartDate = dbEmployee.CareerStartDate,
            Gender = dbEmployee.Gender
        };
    }
}