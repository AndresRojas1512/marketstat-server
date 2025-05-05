using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimFederalDistrictService;

public interface IDimFederalDistrictService
{
    Task<DimFederalDistrict> CreateDistrictAsync(string districtName);
    Task<DimFederalDistrict> GetDistrictByIdAsync(int id);
    Task<IEnumerable<DimFederalDistrict>> GetAllDistrictsAsync();
    Task<DimFederalDistrict> UpdateDistrictAsync(int districtId, string districtName);
    Task DeleteDistrictAsync(int id);
}