using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimFederalDistrictRepository
{
    Task AddFederalDistrictAsync(DimFederalDistrict district);
    Task<DimFederalDistrict> GetFederalDistrictByIdAsync(int id);
    Task<IEnumerable<DimFederalDistrict>> GetAllFederalDistrictsAsync();
    Task UpdateFederalDistrictAsync(DimFederalDistrict district);
    Task DeleteFederalDistrictAsync(int id);
}