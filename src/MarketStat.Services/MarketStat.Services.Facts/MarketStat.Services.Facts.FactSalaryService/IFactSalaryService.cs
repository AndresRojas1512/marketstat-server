using MarketStat.Common.Dto.MarketStat.Common.Dto.Facts;

namespace MarketStat.Services.Facts.FactSalaryService;

public interface IFactSalaryService
{
    Task<decimal> GetAverageSalaryAsync(FactSalaryFilter filter);
}