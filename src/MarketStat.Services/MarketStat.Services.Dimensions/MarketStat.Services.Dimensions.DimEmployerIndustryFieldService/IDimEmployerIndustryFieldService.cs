using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;

public interface IDimEmployerIndustryFieldService
{
    Task<DimEmployerIndustryField> CreateEmployerIndustryFieldAsync(int employerId, int industryFieldId);
    Task<DimEmployerIndustryField> GetEmployerIndustryFieldAsync(int employerId, int industryFieldId);
    Task<IEnumerable<DimEmployerIndustryField>> GetIndustryFieldsByEmployerIdAsync(int employerId);
    Task<IEnumerable<DimEmployerIndustryField>> GetEmployersByIndustryFieldIdAsync(int industryFieldId);
    Task<IEnumerable<DimEmployerIndustryField>> GetAllEmployerIndustryFieldsAsync();
    Task DeleteEmployerIndustryFieldAsync(int employerId, int industryFieldId);
}