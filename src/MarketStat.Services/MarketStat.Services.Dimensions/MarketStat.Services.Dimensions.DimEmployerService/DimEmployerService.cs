namespace MarketStat.Services.Dimensions.DimEmployerService;

using MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService.Validators;
using Microsoft.Extensions.Logging;

public class DimEmployerService : IDimEmployerService
{
    private readonly IDimEmployerRepository _dimEmployerRepository;
    private readonly ILogger<DimEmployerService> _logger;

    public DimEmployerService(IDimEmployerRepository dimEmployerRepository, ILogger<DimEmployerService> logger)
    {
        _dimEmployerRepository = dimEmployerRepository ?? throw new ArgumentNullException(nameof(dimEmployerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DimEmployer> CreateEmployerAsync(string employerName, string inn, string ogrn, string kpp, DateOnly registrationDate, string legalAddress, string contactEmail, string contactPhone, int industryFieldId)
    {
        DimEmployerValidator.ValidateForCreate(employerName, inn, ogrn, kpp, registrationDate, legalAddress, contactEmail, contactPhone, industryFieldId);
        _logger.LogInformation("Attempting to create employer: {EmployerName}", employerName);

        var employerDomain = new DimEmployer
        {
            EmployerName = employerName,
            Inn = inn,
            Ogrn = ogrn,
            Kpp = kpp,
            RegistrationDate = registrationDate,
            LegalAddress = legalAddress,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone,
            IndustryFieldId = industryFieldId,
        };

        try
        {
            await _dimEmployerRepository.AddEmployerAsync(employerDomain).ConfigureAwait(false);
            _logger.LogInformation("Created DimEmployer {EmployerId} ('{EmployerName}')", employerDomain.EmployerId, employerDomain.EmployerName);
            return employerDomain;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating employer '{EmployerName}'. It might already exist.", employerName);
            throw;
        }
    }

    public async Task<DimEmployer> GetEmployerByIdAsync(int employerId)
    {
        try
        {
            return await _dimEmployerRepository.GetEmployerByIdAsync(employerId).ConfigureAwait(false);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Employer {EmployerId} not found", employerId);
            throw;
        }
    }

    public async Task<IEnumerable<DimEmployer>> GetAllEmployersAsync()
    {
        var employers = await _dimEmployerRepository.GetAllEmployersAsync().ConfigureAwait(false);
        _logger.LogInformation("Fetched {Count} Employer records", employers.Count());
        return employers;
    }

    public async Task<DimEmployer> UpdateEmployerAsync(int employerId, string employerName, string inn, string ogrn, string kpp, DateOnly registrationDate, string legalAddress, string contactEmail, string contactPhone, int industryFieldId)
    {
        DimEmployerValidator.ValidateForUpdate(employerId, employerName, inn, ogrn, kpp, registrationDate, legalAddress, contactEmail, contactPhone, industryFieldId);

        _logger.LogInformation("Attempting to update DimEmployer {EmployerId}", employerId);

        try
        {
            var existingEmployer = await _dimEmployerRepository.GetEmployerByIdAsync(employerId).ConfigureAwait(false);
            existingEmployer.EmployerName = employerName;
            existingEmployer.Inn = inn;
            existingEmployer.Ogrn = ogrn;
            existingEmployer.Kpp = kpp;
            existingEmployer.RegistrationDate = registrationDate;
            existingEmployer.LegalAddress = legalAddress;
            existingEmployer.ContactEmail = contactEmail;
            existingEmployer.ContactPhone = contactPhone;
            existingEmployer.IndustryFieldId = industryFieldId;

            await _dimEmployerRepository.UpdateEmployerAsync(existingEmployer).ConfigureAwait(false);
            _logger.LogInformation("Updated DimEmployer {EmployerId}", employerId);
            return existingEmployer;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: employer {EmployerId} not found.", employerId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict when updating employer {EmployerId}.", employerId);
            throw;
        }
    }

    public async Task DeleteEmployerAsync(int employerId)
    {
        try
        {
            await _dimEmployerRepository.DeleteEmployerAsync(employerId).ConfigureAwait(false);
            _logger.LogInformation("Deleted DimEmployer {EmployerId}", employerId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete, employer {EmployerId} not found", employerId);
            throw;
        }
    }
}
