using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimOblastService;

public interface IDimOblastService
{
    Task<DimOblast> CreateOblastAsync(string oblastName, int districtId);
    Task<DimOblast> GetOblastByIdAsync(int oblastId);
    Task<IEnumerable<DimOblast>> GetAllOblastsAsync();
    Task<IEnumerable<DimOblast>> GetOblastsByFederalDistrictIdAsync(int districtId);
    Task<DimOblast> UpdateOblastAsync(int oblastId, string oblastName, int districtId);
    Task DeleteOblastAsync(int id);
}
