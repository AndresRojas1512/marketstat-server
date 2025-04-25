using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimDateRepository
{
    Task AddDateAsync(DimDate employer);
    Task<DimDate> GetDateByIdAsync(int dateId);
    Task<IEnumerable<DimDate>> GetAllDatesAsync();
    Task UpdateDateAsync(DimDate date);
    Task DeleteDateAsync(int dateId);
}