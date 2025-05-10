using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimIndustryFieldService.Validators;
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
    
    public async Task<DimIndustryField> CreateIndustryFieldAsync(string industryFieldName)
    {
        DimIndustryFieldValidator.ValidateForCreate(industryFieldName);
        var industryField = new DimIndustryField(0, industryFieldName);

        try
        {
            await _dimIndustryFieldRepository.AddIndustryFieldAsync(industryField);
            _logger.LogInformation("Created DimIndustryField {IndustryFieldId}", industryField.IndustryFieldId);
            return industryField;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict creating industry field {IndustryFieldId} with name {IndustryFieldName}.",
                industryField.IndustryFieldId, industryField.IndustryFieldName);
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
        var industryFields = await _dimIndustryFieldRepository.GetAllIndustryFieldsAsync();
        _logger.LogInformation("Fetched {Count} industry fields", industryFields.Count());
        return industryFields;
    }
    
    public async Task<DimIndustryField> UpdateIndustryFieldAsync(int industryFieldId, string industryFieldName)
    {
        DimIndustryFieldValidator.ValidateForUpdate(industryFieldId, industryFieldName);
        try
        {
            var existingIndustryField = await _dimIndustryFieldRepository.GetIndustryFieldByIdAsync(industryFieldId);
            
            existingIndustryField.IndustryFieldName = industryFieldName;
            
            await _dimIndustryFieldRepository.UpdateIndustryFieldAsync(existingIndustryField);
            _logger.LogInformation("Updated DimIndustryField {IndustryFieldId}", industryFieldId);
            return existingIndustryField;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Industry field {IndustryFieldId} not found", industryFieldId);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict updating industry field {IndustryFieldId}", industryFieldName);
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
}