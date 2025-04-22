using MarketStat.Common.Core.MarketStat.Common.Core.Facts;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

namespace MarketStat.Database.Core.Repositories.Facts;

public interface IFactSalaryRepository
{
    Task<IEnumerable<FactSalary>> GetSalaryByFilterAsync(FactSalaryFilter salaryFilter);
}