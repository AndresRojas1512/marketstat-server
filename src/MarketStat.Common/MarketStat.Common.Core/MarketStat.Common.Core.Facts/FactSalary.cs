namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts;

public class FactSalary
{
    public long SalaryFactId { get; set; }
    public int DateId { get; set; }
    public int CityId { get; set; }
    public int EmployerId { get; set; }
    public int JobRoleId { get; set; }
    public int EmployeeId { get; set; }
    public decimal SalaryAmount { get; set; }
    public decimal BonusAmount { get; set; }

    public FactSalary(long salaryFactId, int dateId, int cityId, int employerId, int jobRoleId, int employeeId,
        decimal salaryAmount, decimal bonusAmount)
    {
        SalaryFactId = salaryFactId;
        DateId = dateId;
        CityId = cityId;
        EmployerId = employerId;
        JobRoleId = jobRoleId;
        EmployeeId = employeeId;
        SalaryAmount = salaryAmount;
        BonusAmount = bonusAmount;
    }
}