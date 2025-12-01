using MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimDateService;

public interface IDimDateService
{
    Task<DimDate> CreateDateAsync(DateOnly fullDate);

    Task<DimDate> GetDateByIdAsync(int dateId);

    Task<IEnumerable<DimDate>> GetAllDatesAsync();

    Task<DimDate> UpdateDateAsync(int dateId, DateOnly fullDate);

    Task DeleteDateAsync(int dateId);
}
