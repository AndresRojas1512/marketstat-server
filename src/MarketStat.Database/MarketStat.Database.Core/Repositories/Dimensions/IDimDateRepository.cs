namespace MarketStat.Database.Core.Repositories.Dimensions;

using MarketStat.Common.Core.Dimensions;

public interface IDimDateRepository
{
    Task AddDateAsync(DimDate dimDate);

    Task<DimDate> GetDateByIdAsync(int dateId);

    Task<IEnumerable<DimDate>> GetAllDatesAsync();

    Task UpdateDateAsync(DimDate dimDate);

    Task DeleteDateAsync(int dateId);
}
