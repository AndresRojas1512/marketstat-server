namespace MarketStat.Contracts.Facts;

public interface IGetFactSalaryResponse
{
    long SalaryFactId { get; }
    int DateId { get; }
    int LocationId { get; }
    int EmployerId { get; }
    int JobId { get; }
    int EmployeeId { get; }
    decimal SalaryAmount { get; }
}

public interface IFactSalaryNotFoundResponse
{
    long SalaryFactId { get; }
}