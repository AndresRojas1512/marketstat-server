namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

public record FactSalaryDto(
    int SalaryFactId,
    int DateId,
    int CityId,
    int EmployerId,
    int JobRoleId,
    int EmployeeId,
    decimal SalaryAmount,
    decimal BonusAmount
);