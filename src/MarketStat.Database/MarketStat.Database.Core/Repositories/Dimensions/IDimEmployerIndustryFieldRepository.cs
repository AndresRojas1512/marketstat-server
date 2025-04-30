using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimEmployerIndustryFieldRepository
{
    Task AddEmployerIndustryFieldAsync(DimEmployerIndustryField link);
    Task<DimEmployerIndustryField> GetEmployerIndustryFieldAsync(int employerId, int industryFieldId);
    Task<IEnumerable<DimEmployerIndustryField>> GetIndustryFieldsByEmployerIdAsync(int employerId);
    Task<IEnumerable<DimEmployerIndustryField>> GetEmployersByIndustryFieldIdAsync(int industryFieldId);
    Task<IEnumerable<DimEmployerIndustryField>> GetAllEmployerIndustryFieldsAsync();
    Task DeleteEmployerIndustryFieldAsync(int employerId, int industryFieldId);
}