using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;
using MarketStat.Common.Enums;

namespace MarketStat.Database.Core.Repositories.Facts;

public interface IFactSalaryRepository
{
    Task AddFactSalaryAsync(FactSalary salary);
    Task<FactSalary> GetFactSalaryByIdAsync(int salaryId);
    public Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(FactSalaryFilter salaryFilter);
    Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync();
    Task UpdateFactSalaryAsync(FactSalary salaryFact);
    Task DeleteFactSalaryByIdAsync(int salaryFactId);
    Task<SalaryStats> GetSalaryStatsAsync(FactSalaryFilter filter);
    Task<IReadOnlyList<(DateOnly Date, decimal AvgSalary)>> GetAverageTimeSeriesAsync(FactSalaryFilter filter, TimeGranularity  granularity);
}