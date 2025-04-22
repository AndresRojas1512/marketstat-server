using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Database.Core.Repositories.Dimensions;

public interface IDimEmployerRepository
{
    Task AddEmployerAsync(DimEmployer employer);
    Task<DimEmployer> GetEmployerByIdAsync(int employerId);
    Task<IEnumerable<DimEmployer>> GetAllEmployersAsync();
    Task UpdateEmployerAsync(DimEmployer employer);
    Task DeleteEmployerAsync(int employerId);
}