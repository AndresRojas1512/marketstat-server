using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimIndustryFieldRepository
{
    Task AddIndustryFieldAsync(DimIndustryField industryField);
    Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId);
    Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync();
    Task UpdateIndustryFieldAsync(DimIndustryField industryField);
    Task DeleteIndustryFieldAsync(int industryFieldId);
}