using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEducationService.Validators;
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
    
    public async Task<DimEducation> CreateEducationAsync(string specialty, string specialtyCode, int educationLevelId, int industryFieldId)
    {
        DimEducationValidator.ValidateForCreate(specialty, specialtyCode, educationLevelId, industryFieldId);
        var education = new DimEducation(0, specialty, specialtyCode, educationLevelId, industryFieldId);
        try
        {
            await _dimEducationRepository.AddEducationAsync(education);
            _logger.LogInformation("Created DimEducation {EducationID}", education.EducationId);
            return education;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create education (duplicate {Id})", education.EducationId);
            throw new Exception($"An education record with ID {education.EducationId} already exists.");
        }
    }
    
    public async Task<DimEducation> GetEducationByIdAsync(int educationId)
    {
        try
        {
            return await _dimEducationRepository.GetEducationByIdAsync(educationId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Education {Id} not found", educationId);
            throw new Exception($"Education with ID {educationId} was not found.");
        }
    }
    
    public async Task<IEnumerable<DimEducation>> GetAllEducationsAsync()
    {
        var list = await _dimEducationRepository.GetAllEducationsAsync();
        _logger.LogInformation("Fetched {Count} education records", list.Count());
        return list;
    }
    
    public async Task<DimEducation> UpdateEducationAsync(int educationId, string specialty, string specialtyCode, int educationLevelId, int industryFieldId)
    {
        DimEducationValidator.ValidateForUpdate(educationId, specialty, specialtyCode, educationLevelId, industryFieldId);
        try
        {
            var existing = await _dimEducationRepository.GetEducationByIdAsync(educationId);
            existing.Specialty = specialty;
            existing.SpecialtyCode = specialtyCode;
            existing.EducationLevelId = educationLevelId;
            existing.IndustryFieldId = industryFieldId;

            await _dimEducationRepository.UpdateEducationAsync(existing);
            _logger.LogInformation("Updated education {Id}", educationId);
            return existing;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update – education {Id} not found", educationId);
            throw new Exception($"Cannot update: education {educationId} was not found.");
        }
    }
    
    public async Task DeleteEducationAsync(int educationId)
    {
        try
        {
            await _dimEducationRepository.DeleteEducationAsync(educationId);
            _logger.LogInformation("Deleted education {Id}", educationId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete – education {Id} not found", educationId);
            throw new Exception($"Cannot delete: education {educationId} not found.");
        }
    }
}