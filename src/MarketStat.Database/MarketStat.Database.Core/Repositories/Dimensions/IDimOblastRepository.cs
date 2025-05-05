using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimOblastRepository
{
    Task AddOblastAsync(DimOblast dimOblast);
    Task<DimOblast> GetOblastByIdAsync(int id);
    Task<IEnumerable<DimOblast>> GetAllOblastsAsync();
    Task<IEnumerable<DimOblast>> GetOblastsByFederalDistrictIdAsync(int id);
    Task UpdateOblastAsync(DimOblast dimOblast);
    Task DeleteOblastAsync(int id);
}