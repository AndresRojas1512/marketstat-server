namespace MarketStat.Contracts.Facts;

public interface ISubmitFactSalaryCommand
{
    int DateId { get; }
    int LocationId { get; }
    int EmployerId { get; }
    int JobId { get; }
    int EmployeeId { get; }
    decimal SalaryAmount { get; }
}