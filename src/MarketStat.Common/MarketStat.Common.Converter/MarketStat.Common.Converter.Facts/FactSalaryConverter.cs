using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;

public static class FactSalaryConverter
{
    public static FactSalaryDbModel ToDbModel(FactSalary salary)
    {
        return new FactSalaryDbModel(
            salaryFactId: salary.SalaryFactId,
            dateId: salary.DateId,
            cityId: salary.CityId,
            employerId: salary.EmployerId,
            jobRoleId: salary.JobRoleId,
            employeeId: salary.EmployeeId,
            salaryAmount: salary.SalaryAmount,
            bonusAmount: salary.BonusAmount
        );
    }

    public static FactSalary ToDomain(FactSalaryDbModel dbSalary)
    {
        return new FactSalary(
            salaryFactId: dbSalary.SalaryFactId,
            dateId: dbSalary.DateId,
            cityId: dbSalary.CityId,
            employerId: dbSalary.EmployerId,
            jobRoleId: dbSalary.JobRoleId,
            employeeId: dbSalary.EmployeeId,
            salaryAmount: dbSalary.SalaryAmount,
            bonusAmount: dbSalary.BonusAmount
        );
    }
}