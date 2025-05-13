using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;

namespace MarketStat.Services.Facts.FactSalaryService;

public interface IFactSalaryService
{
    Task<FactSalary> CreateFactSalaryAsync(int dateId, int cityId, int employerId, int jobRoleId, int employeeId,
        decimal salaryAmount, decimal bonusAmount);
    Task<FactSalary> GetFactSalaryByIdAsync(int salaryFactId);
    Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync();
    Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(FactSalaryFilter filter);
    Task<FactSalary> UpdateFactSalaryAsync(int salaryFactId, int dateId, int cityId, int employerId, int jobRoleId,
        int employeeId, decimal salaryAmount, decimal bonusAmount);
    Task DeleteFactSalaryAsync(int salaryFactId);
    Task<decimal> GetAverageSalaryAsync(FactSalaryFilter filter);
    Task<SalaryStats> GetSalaryStatsAsync(FactSalaryFilter filter);
    Task<IReadOnlyList<(DateOnly Date, decimal AvgSalary)>> GetAverageTimeSeriesAsync(FactSalaryFilter filter, TimeGranularity  granularity);
}