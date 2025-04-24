using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimIndustryFieldService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimIndustryFieldService;

public class DimIndustryFieldService
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
        var all = (await _dimIndustryFieldRepository.GetAllIndustryFieldsAsync()).ToList();
        var newId = all.Any() ? all.Max(f => f.IndustryFieldId) + 1 : 1;

        DimIndustryFieldValidator.ValidateParameters(newId, industryFieldName, checkId: false);
        var field = new DimIndustryField(newId, industryFieldName);

        try
        {
            await _dimIndustryFieldRepository.AddIndustryFieldAsync(field);
            _logger.LogInformation("Created DimIndustryField {IndustryFieldId}", newId);
            return field;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimIndustryField {IndustryFieldId}", newId);
            throw new Exception($"Could not create industry field {industryFieldName}");
        }
    }
    
    public async Task<DimIndustryField> GetIndustryFieldByIdAsync(int industryFieldId)
    {
        try
        {
            return await _dimIndustryFieldRepository.GetIndustryFieldByIdAsync(industryFieldId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IndustryField {IndustryFieldId} not found", industryFieldId);
            throw new Exception($"Industry field {industryFieldId} was not found.");
        }
    }
    
    public async Task<IEnumerable<DimIndustryField>> GetAllIndustryFieldsAsync()
    {
        var list = await _dimIndustryFieldRepository.GetAllIndustryFieldsAsync();
        _logger.LogInformation("Fetched {Count} industry fields", list.Count());
        return list;
    }
    
    public async Task<DimIndustryField> UpdateIndustryFieldAsync(int industryFieldId, string industryFieldName)
    {
        DimIndustryFieldValidator.ValidateParameters(industryFieldId, industryFieldName);
        try
        {
            var existing = await _dimIndustryFieldRepository.GetIndustryFieldByIdAsync(industryFieldId);
            existing.IndustryFieldName = industryFieldName;
            await _dimIndustryFieldRepository.UpdateIndustryFieldAsync(existing);
            _logger.LogInformation("Updated DimIndustryField {IndustryFieldId}", industryFieldId);
            return existing;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update industry field {IndustryFieldId}", industryFieldId);
            throw new Exception($"Cannot update: industry field {industryFieldId} not found.");
        }
    }
    
    public async Task DeleteIndustryFieldAsync(int industryFieldId)
    {
        try
        {
            await _dimIndustryFieldRepository.DeleteIndustryFieldAsync(industryFieldId);
            _logger.LogInformation("Deleted DimIndustryField {IndustryFieldId}", industryFieldId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete industry field {IndustryFieldId}", industryFieldId);
            throw new Exception($"Cannot delete: industry field {industryFieldId} not found.");
        }
    }
}