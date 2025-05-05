using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimEmployerIndustryFieldService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimEmployerIndustryFieldService;

public class DimEmployerIndustryFieldService : IDimEmployerIndustryFieldService
{
    private readonly IDimEmployerIndustryFieldRepository _dimEmployerIndustryFieldRepository;
    private readonly ILogger<DimEmployerIndustryFieldService> _logger;

    public DimEmployerIndustryFieldService(IDimEmployerIndustryFieldRepository dimEmployerIndustryFieldRepository,
        ILogger<DimEmployerIndustryFieldService> logger)
    {
        _dimEmployerIndustryFieldRepository = dimEmployerIndustryFieldRepository;
        _logger = logger;
    }

    public async Task<DimEmployerIndustryField> CreateEmployerIndustryFieldAsync(int employerId, int industryFieldId)
    {
        DimEmployerIndustryFieldValidator.ValidateParameters(employerId, industryFieldId);
        var link = new DimEmployerIndustryField(employerId, industryFieldId);
        try
        {
            await _dimEmployerIndustryFieldRepository.AddEmployerIndustryFieldAsync(link);
            _logger.LogInformation("Created link EmployerIndustryField ({EmployerId}, {IndustryFieldId}).", employerId,
                industryFieldId);
            return link;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create link EmployerIndustryField ({EmployerId}, {IndustryFieldId}).",
                employerId, industryFieldId);
            throw new Exception($"Link ({employerId}, {industryFieldId}) already exists.");
        }
    }

    public async Task<DimEmployerIndustryField> GetEmployerIndustryFieldAsync(int employerId, int industryFieldId)
    {
        try
        {
            return await _dimEmployerIndustryFieldRepository.GetEmployerIndustryFieldAsync(employerId, industryFieldId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Link EmployerIndustryField ({EmployerId}, {IndustryFieldId}) not found.", employerId,
                industryFieldId);
            throw new Exception($"Link EmployerIndustryField ({employerId}, {industryFieldId}) not found.");
        }
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetIndustryFieldsByEmployerIdAsync(int employerId)
    {
        var list = await _dimEmployerIndustryFieldRepository.GetIndustryFieldsByEmployerIdAsync(employerId);
        _logger.LogInformation("Fetched {Count} industries for employer {EmployerId},", list.Count(), employerId);
        return list;
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetEmployersByIndustryFieldIdAsync(int industryFieldId)
    {
        var list = await _dimEmployerIndustryFieldRepository.GetEmployersByIndustryFieldIdAsync(industryFieldId);
        _logger.LogInformation("Fetched {Count} employers for industry field {IndustryFieldId}.", list.Count(), industryFieldId);
        return list;
    }

    public async Task<IEnumerable<DimEmployerIndustryField>> GetAllEmployerIndustryFieldsAsync()
    {
        var list = (await _dimEmployerIndustryFieldRepository.GetAllEmployerIndustryFieldsAsync()).ToList();
        _logger.LogInformation("Fetched {Count} total EmployerIndustryField links.", list.Count);
        return list;
    }

    public async Task DeleteEmployerIndustryFieldAsync(int employerId, int industryFieldId)
    {
        try
        {
            await _dimEmployerIndustryFieldRepository.DeleteEmployerIndustryFieldAsync(employerId, industryFieldId);
            _logger.LogInformation("Deleted link EmployerIndustryField ({EmployerId}, {IndustryFieldId}).", employerId, industryFieldId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete EmployerIndustryField link ({EmployerId}, {IndustryFieldId}).", employerId, industryFieldId);
            throw new Exception($"Cannot delete EmployerIndustryField link ({employerId}, {industryFieldId}).");
        }
    }
}