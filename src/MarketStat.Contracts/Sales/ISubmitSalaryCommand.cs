namespace MarketStat.Contracts.Sales;

public interface ISubmitSalaryCommand
{
    int DateId { get; }
    int LocationId { get; }
    int EmployerId { get; }
    int JobId { get; }
    int EmployeeId { get; }
    decimal SalaryAmount { get; }
}