namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts;

public class FactSalary
{
    public int SalaryFactId { get; set; }
    public int DateId { get; set; }
    public int CityId { get; set; }
    public int EmployerId { get; set; }
    public int JobRoleId { get; set; }
    public int EmployeeId { get; set; }
    public int SalaryAmount { get; set; }
    public int BonusAmount { get; set; }

    public FactSalary(int salaryFactId, int dateId, int cityId, int employerId, int jobRoleId, int employeeId,
        int salaryAmount, int bonusAmount)
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