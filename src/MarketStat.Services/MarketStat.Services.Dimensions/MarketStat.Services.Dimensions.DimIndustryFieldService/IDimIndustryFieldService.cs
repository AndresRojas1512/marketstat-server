using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimIndustryFieldService;

public interface IDimIndustryFieldService
{
    Task<DimIndustryField> CreateIndustryFieldAsync(string industryFieldName);
    Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId);
    Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync();
    Task<DimIndustryField> UpdateIndustryFieldAsync(int industryFieldId, string industryFieldName);
    Task DeleteIndustryFieldAsync(int industryFieldId);
}