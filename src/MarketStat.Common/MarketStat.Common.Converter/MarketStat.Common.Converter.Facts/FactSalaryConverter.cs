using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Database.Models.MarketStat.Database.Models.Facts;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Facts;

public static class FactSalaryConverter
{
    public static FactSalaryDbModel ToDbModel(FactSalary salary)
    {
        if (salary == null)
            throw new ArgumentNullException(nameof(salary));

        return new FactSalaryDbModel(
            dateId: salary.DateId,
            cityId: salary.CityId,
            employerId: salary.EmployerId,
            jobRoleId: salary.JobRoleId,
            employeeId: salary.EmployeeId,
            salaryAmount: salary.SalaryAmount,
            bonusAmount: salary.BonusAmount,
            salaryFactId: salary.SalaryFactId
        );
    }

    public static FactSalary ToDomain(FactSalaryDbModel dbSalary)
    {
        if (dbSalary == null)
            throw new ArgumentNullException(nameof(dbSalary));

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

    public static IEnumerable<FactSalary> ToDomainList(IEnumerable<FactSalaryDbModel> dbSalaries)
    {
        if (dbSalaries == null)
            return Enumerable.Empty<FactSalary>();
        
        return dbSalaries.Select(ToDomain);
    }

    public static IEnumerable<FactSalaryDbModel> ToDbModelList(IEnumerable<FactSalary> salaries)
    {
        if (salaries == null)
            return Enumerable.Empty<FactSalaryDbModel>();

        return salaries.Select(ToDbModel);
    }
}