using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimIndustryFieldService;

public interface IDimIndustryFieldService
{
    Task<DimIndustryField> CreateIndustryFieldAsync(string industryFieldCode, string industryFieldName);
    Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId);
    Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync();
    Task<DimIndustryField> UpdateIndustryFieldAsync(int industryFieldId, string industryFieldCode, string industryFieldName);
    Task DeleteIndustryFieldAsync(int industryFieldId);
    Task<DimIndustryField?> GetIndustryFieldByNameAsync(string industryFieldName);
}