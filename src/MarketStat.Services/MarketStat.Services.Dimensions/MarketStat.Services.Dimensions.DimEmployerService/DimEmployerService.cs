using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEmployerService;

public class DimEmployerService : IDimEmployerService
{
    private readonly IDimEmployerRepository _dimEmployerRepository;
    private readonly ILogger<DimEmployerService> _logger;

    public DimEmployerService(IDimEmployerRepository dimEmployerRepository, ILogger<DimEmployerService> logger)
    {
        _dimEmployerRepository = dimEmployerRepository;
        _logger = logger;
    }

    public async Task<DimEmployer> CreateEmployerAsync(string employerName, bool isPublic)
    {
        DimEmployerValidator.ValidateForCreate(employerName, isPublic);
        var employer = new DimEmployer(0, employerName, isPublic);
        
        try
        {
            await _dimEmployerRepository.AddEmployerAsync(employer);
            _logger.LogInformation("Created DimEmployer {EmployerId}", employer.EmployerId);
            return employer;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Failed to create DimEmployer {EmployerId}.", employer.EmployerId);
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

    public async Task<DimEmployer> UpdateEmployerAsync(int employerId, string employerName, bool isPublic)
    {
        DimEmployerValidator.ValidateForUpdate(employerId, employerName, isPublic);
        try
        {
            var existingEmployer = await _dimEmployerRepository.GetEmployerByIdAsync(employerId);
            existingEmployer.EmployerName = employerName;
            existingEmployer.IsPublic = isPublic;
            await _dimEmployerRepository.UpdateEmployerAsync(existingEmployer);
            _logger.LogInformation("Updated DimEmployer {EmployerId}", employerId);
            return existingEmployer;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: employer {EmployerId} not found", employerId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict when updating employer {EmployerId}", employerId);
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