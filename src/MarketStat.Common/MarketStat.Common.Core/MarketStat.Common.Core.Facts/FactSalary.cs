namespace MarketStat.Common.Core.MarketStat.Common.Core.Facts;

public class FactSalary
{
    public long SalaryFactId { get; set; }
    public int DateId { get; set; }
    public int LocationId { get; set; }
    public int EmployerId { get; set; }
    public int JobId { get; set; }
    public int EmployeeId { get; set; }
    public decimal SalaryAmount { get; set; }
    
    public FactSalary() { }

    public FactSalary(long salaryFactId, int dateId, int locationId, int employerId, int jobId, int employeeId,
        decimal salaryAmount)
    {
        SalaryFactId = salaryFactId;
        DateId = dateId;
        LocationId = locationId;
        EmployerId = employerId;
        JobId = jobId;
        EmployeeId = employeeId;
        SalaryAmount = salaryAmount;
    }
}