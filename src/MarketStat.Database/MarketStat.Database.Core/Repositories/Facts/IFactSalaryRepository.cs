using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

namespace MarketStat.Database.Core.Repositories.Facts;

public interface IFactSalaryRepository
{
    Task AddFactSalaryAsync(FactSalary salary);
    Task<FactSalary> GetFactSalaryByIdAsync(int salaryId);
    public Task<IEnumerable<FactSalary>> GetFactSalariesByFilterAsync(FactSalaryFilter salaryFilter);
    Task<IEnumerable<FactSalary>> GetAllFactSalariesAsync();
    Task UpdateFactSalaryAsync(FactSalary salaryFact);
    Task DeleteFactSalaryByIdAsync(int salaryFactId);
}