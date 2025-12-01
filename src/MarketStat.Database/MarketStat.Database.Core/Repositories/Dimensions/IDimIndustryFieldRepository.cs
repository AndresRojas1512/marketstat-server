namespace MarketStat.Database.Core.Repositories.Dimensions;

using MarketStat.Common.Core.Dimensions;

public interface IDimIndustryFieldRepository
{
    Task AddIndustryFieldAsync(DimIndustryField industryField);

    Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId);

    Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync();

    Task UpdateIndustryFieldAsync(DimIndustryField industryField);

    Task DeleteIndustryFieldAsync(int industryFieldId);

    Task<DimIndustryField?> GetIndustryFieldByNameAsync(string industryFieldName);
}
