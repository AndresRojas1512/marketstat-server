using MarketStat.Common.Core.Facts;
using MarketStat.Database.Models.Facts;

namespace MarketStat.Common.Converter.Facts;

public static class FactSalaryConverter
{
    public static FactSalaryDbModel ToDbModel(FactSalary salary)
    {
        ArgumentNullException.ThrowIfNull(salary);

        return new FactSalaryDbModel
        {
            SalaryFactId = salary.SalaryFactId,
            DateId = salary.DateId,
            LocationId = salary.LocationId,
            EmployerId = salary.EmployerId,
            JobId = salary.JobId,
            EmployeeId = salary.EmployeeId,
            SalaryAmount = salary.SalaryAmount,
        };
    }

    public static FactSalary ToDomain(FactSalaryDbModel dbSalary)
    {
        ArgumentNullException.ThrowIfNull(dbSalary);

        return new FactSalary(
            salaryFactId: dbSalary.SalaryFactId,
            dateId: dbSalary.DateId,
            locationId: dbSalary.LocationId,
            employerId: dbSalary.EmployerId,
            jobId: dbSalary.JobId,
            employeeId: dbSalary.EmployeeId,
            salaryAmount: dbSalary.SalaryAmount);
    }

    public static IEnumerable<FactSalary> ToDomainList(IEnumerable<FactSalaryDbModel> dbSalaries)
    {
        if (dbSalaries == null)
        {
            return Enumerable.Empty<FactSalary>();
        }

        return dbSalaries.Select(ToDomain);
    }

    public static IEnumerable<FactSalaryDbModel> ToDbModelList(IEnumerable<FactSalary> salaries)
    {
        if (salaries == null)
        {
            return Enumerable.Empty<FactSalaryDbModel>();
        }

        return salaries.Select(ToDbModel);
    }
}
