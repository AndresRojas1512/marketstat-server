using MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Dimensions;

public interface IDimEmployerService
{
    Task<DimEmployer> CreateEmployerAsync(string employerName, string inn, string ogrn, string kpp, DateOnly registrationDate, string legalAddress, string contactEmail, string contactPhone, int industryFieldId);

    Task<DimEmployer> GetEmployerByIdAsync(int employerId);

    Task<IEnumerable<DimEmployer>> GetAllEmployersAsync();

    Task<DimEmployer> UpdateEmployerAsync(int employerId, string employerName, string inn, string ogrn, string kpp, DateOnly registrationDate, string legalAddress, string contactEmail, string contactPhone, int industryFieldId);

    Task DeleteEmployerAsync(int employerId);
}
