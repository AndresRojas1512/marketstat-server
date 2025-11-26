using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Common.Validators.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEducationService;

public class DimEducationService : IDimEducationService
{
    private readonly IDimEducationRepository _dimEducationRepository;
    private readonly ILogger<DimEducationService> _logger;

    public DimEducationService(IDimEducationRepository dimEducationRepository, ILogger<DimEducationService> logger)
    {
        _dimEducationRepository = dimEducationRepository;
        _logger = logger;
    }
    
    public async Task<DimEducation> CreateEducationAsync(string specialtyName, string specialtyCode, string educationLevelName)
    {
        DimEducationValidator.ValidateForCreate(specialtyName, specialtyCode, educationLevelName);
        var education = new DimEducation(0, specialtyName, specialtyCode, educationLevelName);
        try
        {
            await _dimEducationRepository.AddEducationAsync(education);
            _logger.LogInformation("Created education {EducationID}", education.EducationId);
            return education;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating education {EducationId} with code {SpecialtyCode}",
                education.EducationId, education.SpecialtyCode);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Cannot create education {SpecialtyCode}: FKs not found", education.SpecialtyCode);
            throw;
        }
    }
    
    public async Task<DimEducation> GetEducationByIdAsync(int educationId)
    {
        try
        {
            return await _dimEducationRepository.GetEducationByIdAsync(educationId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Education {Id} not found", educationId);
            throw;
        }
    }
    
    public async Task<IEnumerable<DimEducation>> GetAllEducationsAsync()
    {
        var list = await _dimEducationRepository.GetAllEducationsAsync();
        _logger.LogInformation("Fetched {Count} education records", list.Count());
        return list;
    }
    
    public async Task<DimEducation> UpdateEducationAsync(int educationId, string specialtyName, string specialtyCode, string educationLevelName)
    {
        DimEducationValidator.ValidateForUpdate(educationId, specialtyName, specialtyCode, educationLevelName);
        try
        {
            var existing = await _dimEducationRepository.GetEducationByIdAsync(educationId);

            existing.SpecialtyName = specialtyName;
            existing.SpecialtyCode = specialtyCode;
            existing.EducationLevelName = educationLevelName;

            await _dimEducationRepository.UpdateEducationAsync(existing);
            _logger.LogInformation("Updated education {Id}", educationId);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update: education {Id} not found", educationId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict updating education {Id}, already exists", educationId);
            throw;
        }
    }
    
    public async Task DeleteEducationAsync(int educationId)
    {
        try
        {
            await _dimEducationRepository.DeleteEducationAsync(educationId);
            _logger.LogInformation("Deleted education {Id}", educationId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete: education {Id} not found", educationId);
            throw;
        }
    }
}