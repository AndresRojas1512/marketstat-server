using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEmployerService;

public interface IDimEmployerService
{
    Task<DimEmployer> CreateEmployerAsync(string employerName, bool isPublic);
    Task<DimEmployer> GetEmployerByIdAsync(int employerId);
    Task<IEnumerable<DimEmployer>> GetAllEmployersAsync();
    Task<DimEmployer> UpdateEmployerAsync(int employerId, string employerName, bool isPublic);
    Task DeleteEmployerAsync(int employerId);
}