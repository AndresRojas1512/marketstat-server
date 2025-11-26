using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Common.Validators.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimIndustryFieldService;

public class DimIndustryFieldService : IDimIndustryFieldService
{
    private readonly IDimIndustryFieldRepository _dimIndustryFieldRepository;
    private readonly ILogger<DimIndustryFieldService> _logger;

    public DimIndustryFieldService(IDimIndustryFieldRepository dimIndustryFieldRepository,
        ILogger<DimIndustryFieldService> logger)
    {
        _dimIndustryFieldRepository = dimIndustryFieldRepository;
        _logger = logger;
    }
    
    public async Task<DimIndustryField> CreateIndustryFieldAsync(string industryFieldCode, string industryFieldName)
    {
        DimIndustryFieldValidator.ValidateForCreate(industryFieldCode, industryFieldName);
        _logger.LogInformation("Service: Attempting to create industry field: {IndustryFieldName}", industryFieldName);

        var industryField = new DimIndustryField(0, industryFieldCode, industryFieldName);
        try
        {
            await _dimIndustryFieldRepository.AddIndustryFieldAsync(industryField);
            _logger.LogInformation("Service: Created DimIndustryField {IndustryFieldId} ('{IndustryFieldName}')", 
                industryField.IndustryFieldId, industryField.IndustryFieldName);
            return industryField;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict creating industry field '{IndustryFieldName}'.", industryFieldName);
            throw;
        }
    }
    
    public async Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId)
    {
        try
        {
            return await _dimIndustryFieldRepository.GetIndustryFieldByIdAsync(industryFieldId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Industry field {IndustryFieldId} not found", industryFieldId);
            throw;
        }
    }
    
    public async Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync()
    {
        _logger.LogInformation("Service: Fetching all industry fields.");
        var industryFields = await _dimIndustryFieldRepository.GetAllIndustryFieldsAsync();
        _logger.LogInformation("Service: Fetched {Count} industry fields.", industryFields.Count());
        return industryFields;
    }
    
    public async Task<DimIndustryField> UpdateIndustryFieldAsync(int industryFieldId, string industryFieldCode, string industryFieldName)
    {
        DimIndustryFieldValidator.ValidateForUpdate(industryFieldId, industryFieldCode, industryFieldName);
        _logger.LogInformation("Service: Attempting to update DimIndustryField {IndustryFieldId}", industryFieldId);

        try
        {
            var existingIndustryField = await _dimIndustryFieldRepository.GetIndustryFieldByIdAsync(industryFieldId);
                
            existingIndustryField.IndustryFieldName = industryFieldName;
            existingIndustryField.IndustryFieldCode = industryFieldCode;
                
            await _dimIndustryFieldRepository.UpdateIndustryFieldAsync(existingIndustryField);
            _logger.LogInformation("Service: Updated DimIndustryField {IndustryFieldId}", industryFieldId);
            return existingIndustryField;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Service: Cannot update, industry field {IndustryFieldId} not found.", industryFieldId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict when updating industry field {IndustryFieldId}.", industryFieldId);
            throw;
        }
    }
    
    public async Task DeleteIndustryFieldAsync(int industryFieldId)
    {
        try
        {
            await _dimIndustryFieldRepository.DeleteIndustryFieldAsync(industryFieldId);
            _logger.LogInformation("Deleted DimIndustryField {IndustryFieldId}", industryFieldId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Industry field {IndustryFieldId} not found", industryFieldId);
            throw;
        }
    }

    public async Task<DimIndustryField?> GetIndustryFieldByNameAsync(string industryFieldName)
    {
        _logger.LogInformation("Service: Attempting to find industry field by name: {IndustryFieldName}",
            industryFieldName);
        var industryField = await _dimIndustryFieldRepository.GetIndustryFieldByNameAsync(industryFieldName);
        if (industryField == null)
        {
            _logger.LogWarning("Service: No industry field found with name: {IndustryFieldName}", industryFieldName);
        }
        return industryField;
    }
}