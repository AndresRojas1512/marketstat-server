namespace MarketStat.Database.Core.Repositories.Dimensions;

using MarketStat.Common.Core.Dimensions;

public interface IDimEmployerRepository
{
    Task AddEmployerAsync(DimEmployer employer);

    Task<DimEmployer> GetEmployerByIdAsync(int employerId);

    Task<IEnumerable<DimEmployer>> GetAllEmployersAsync();

    Task UpdateEmployerAsync(DimEmployer employer);

    Task DeleteEmployerAsync(int employerId);
}
