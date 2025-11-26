using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Common.Validators.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEmployerService;

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
            IndustryFieldId = industryFieldId
        };
        
        try
        {
            await _dimEmployerRepository.AddEmployerAsync(employerDomain);
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
            return await _dimEmployerRepository.GetEmployerByIdAsync(employerId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Employer {EmployerId} not found", employerId);
            throw;
        }
    }

    public async Task<IEnumerable<DimEmployer>> GetAllEmployersAsync()
    {
        var employers = await _dimEmployerRepository.GetAllEmployersAsync();
        _logger.LogInformation("Fetched {Count} Employer records", employers.Count());
        return employers;
    }

    public async Task<DimEmployer> UpdateEmployerAsync(int employerId, string employerName, string inn, string ogrn, string kpp, DateOnly registrationDate, string legalAddress, string contactEmail, string contactPhone, int industryFieldId)
    {
        DimEmployerValidator.ValidateForUpdate(employerId, employerName, inn, ogrn, kpp, registrationDate, legalAddress, contactEmail, contactPhone, industryFieldId);
        
        _logger.LogInformation("Attempting to update DimEmployer {EmployerId}", employerId);

        try
        {
            var existingEmployer = await _dimEmployerRepository.GetEmployerByIdAsync(employerId);
            existingEmployer.EmployerName = employerName;
            existingEmployer.Inn = inn;
            existingEmployer.Ogrn = ogrn;
            existingEmployer.Kpp = kpp;
            existingEmployer.RegistrationDate = registrationDate;
            existingEmployer.LegalAddress = legalAddress;
            existingEmployer.ContactEmail = contactEmail;
            existingEmployer.ContactPhone = contactPhone;
            existingEmployer.IndustryFieldId = industryFieldId;

            await _dimEmployerRepository.UpdateEmployerAsync(existingEmployer);
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
            await _dimEmployerRepository.DeleteEmployerAsync(employerId);
            _logger.LogInformation("Deleted DimEmployer {EmployerId}", employerId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete, employer {EmployerId} not found", employerId);
            throw;
        }
    }
}