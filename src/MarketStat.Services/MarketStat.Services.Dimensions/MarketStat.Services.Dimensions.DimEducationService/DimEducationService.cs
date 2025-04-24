using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEducationService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEducationService;

public class DimEducationService
{
    private readonly IDimEducationRepository _dimEducationRepository;
    private readonly ILogger<DimEducationService> _logger;

    public DimEducationService(IDimEducationRepository dimEducationRepository, ILogger<DimEducationService> logger)
    {
        _dimEducationRepository = dimEducationRepository;
        _logger = logger;
    }
    
    public async Task<DimEducation> CreateEducationAsync(string specialization, string educationLevel)
    {
        var all = (await _dimEducationRepository.GetAllEducationsAsync()).ToList();
        int newId = all.Any() ? all.Max(e => e.EducationId) + 1 : 1;

        DimEducationValidator.ValidateParameters(newId, specialization, educationLevel);

        var edu = new DimEducation(newId, specialization, educationLevel);
        try
        {
            await _dimEducationRepository.AddEducationAsync(edu);
            return edu;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create education (duplicate {Id})", newId);
            throw new Exception($"An education record with ID {newId} already exists.");
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
    
    public async Task<DimEducation> UpdateEducationAsync(int educationId, string specialization, string educationLevel)
    {
        try
        {
            DimEducationValidator.ValidateParameters(educationId, specialization, educationLevel);

            var existing = await _dimEducationRepository.GetEducationByIdAsync(educationId);
            existing.Specialization = specialization;
            existing.EducationLevel = educationLevel;

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