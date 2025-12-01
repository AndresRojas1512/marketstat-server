using MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.Dimensions;

public static class DimEmployeeConverter
{
    public static DimEmployeeDbModel ToDbModel(DimEmployee domainEmployee)
    {
        ArgumentNullException.ThrowIfNull(domainEmployee);

        return new DimEmployeeDbModel
        {
            EmployeeId = domainEmployee.EmployeeId,
            EmployeeRefId = domainEmployee.EmployeeRefId,
            BirthDate = domainEmployee.BirthDate,
            CareerStartDate = domainEmployee.CareerStartDate,
            Gender = domainEmployee.Gender,
            EducationId = domainEmployee.EducationId,
            GraduationYear = domainEmployee.GraduationYear,
        };
    }

    public static DimEmployee ToDomain(DimEmployeeDbModel dbEmployee)
    {
        ArgumentNullException.ThrowIfNull(dbEmployee);

        return new DimEmployee
        {
            EmployeeId = dbEmployee.EmployeeId,
            EmployeeRefId = dbEmployee.EmployeeRefId,
            BirthDate = dbEmployee.BirthDate,
            CareerStartDate = dbEmployee.CareerStartDate,
            Gender = dbEmployee.Gender,
            EducationId = dbEmployee.EducationId,
            GraduationYear = dbEmployee.GraduationYear,
        };
    }
}
