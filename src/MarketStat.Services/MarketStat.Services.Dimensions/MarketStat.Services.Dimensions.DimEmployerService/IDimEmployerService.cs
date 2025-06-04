using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions.DimEmployerService;

public interface IDimEmployerService
{
    Task<DimEmployer> CreateEmployerAsync(
        string employerName,
        string inn,
        string ogrn,
        string kpp,
        DateOnly registrationDate,
        string legalAddress,
        string website,
        string contactEmail,
        string contactPhone);
    Task<DimEmployer> GetEmployerByIdAsync(int employerId);
    Task<IEnumerable<DimEmployer>> GetAllEmployersAsync();
    Task<DimEmployer> UpdateEmployerAsync(
        int employerId,
        string employerName,
        string inn,
        string ogrn,
        string kpp,
        DateOnly registrationDate,
        string legalAddress,
        string website,
        string contactEmail,
        string contactPhone);
    Task DeleteEmployerAsync(int employerId);
}