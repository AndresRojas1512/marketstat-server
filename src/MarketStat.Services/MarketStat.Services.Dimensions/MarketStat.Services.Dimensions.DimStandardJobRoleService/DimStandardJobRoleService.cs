using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Services.Dimensions.DimStandardJobRoleService.Validators;
using Microsoft.Extensions.Logging;

namespace MarketStat.Services.Dimensions.DimStandardJobRoleService;

public class DimStandardJobRoleService : IDimStandardJobRoleService
{
    private readonly IDimStandardJobRoleRepository _dimStandardJobRoleRepository;
    private readonly ILogger<DimStandardJobRoleService> _logger;

    public DimStandardJobRoleService(IDimStandardJobRoleRepository dimStandardJobRoleRepository,
        ILogger<DimStandardJobRoleService> logger)
    {
        _dimStandardJobRoleRepository = dimStandardJobRoleRepository;
        _logger = logger;
    }
    
    public async Task<DimStandardJobRole> CreateStandardJobRoleAsync(string standardJobRoleCode, string jobRoleTitle, int industryFieldId)
    {
        DimStandardJobRoleValidator.ValidateForCreate(standardJobRoleCode, jobRoleTitle, industryFieldId);
        _logger.LogInformation("Service: Attempting to create standard job role: {JobRoleTitle}", jobRoleTitle);

        var role = new DimStandardJobRole(0, standardJobRoleCode, jobRoleTitle, industryFieldId);
        try
        {
            await _dimStandardJobRoleRepository.AddStandardJobRoleAsync(role);
            _logger.LogInformation("Service: Created DimStandardJobRole {JobRoleId} ('{JobRoleTitle}')", role.StandardJobRoleId, role.StandardJobRoleTitle);
            return role;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict creating standard job role '{JobRoleTitle}'.", jobRoleTitle);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Service: Cannot create standard job role '{JobRoleTitle}' because IndustryFieldId {IndustryFieldId} was not found.", jobRoleTitle, industryFieldId);
            throw;
        }
    }

    public async Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(int id)
    {
        try
        {
            return await _dimStandardJobRoleRepository.GetStandardJobRoleByIdAsync(id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "StandardJobRole {JobRoleId} not found", id);
            throw;
        }
    }

    public async Task<IEnumerable<DimStandardJobRole>> GetAllStandardJobRolesAsync()
    {
        var list = await _dimStandardJobRoleRepository.GetAllStandardJobRolesAsync();
        _logger.LogInformation("Fetched {Count} JobRole records", list.Count());
        return list;
    }

    public async Task<DimStandardJobRole> UpdateStandardJobRoleAsync(int id, string standardJobRoleCode, string jobRoleTitle, int industryFieldId)
    {
        DimStandardJobRoleValidator.ValidateForUpdate(id, standardJobRoleCode, jobRoleTitle, industryFieldId);
        _logger.LogInformation("Service: Attempting to update DimStandardJobRole {JobRoleId}", id);
        try
        {
            var existing = await _dimStandardJobRoleRepository.GetStandardJobRoleByIdAsync(id);
                
            existing.StandardJobRoleTitle = jobRoleTitle;
            existing.StandardJobRoleCode = standardJobRoleCode;
            existing.IndustryFieldId = industryFieldId;
                
            await _dimStandardJobRoleRepository.UpdateStandardJobRoleAsync(existing);
            _logger.LogInformation("Service: Updated DimStandardJobRole {JobRoleId}", id);
            return existing;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Service: Cannot update, standard job role {JobRoleId} not found.", id);
            throw;
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Service: Conflict updating standard job role {JobRoleId}.", id);
            throw;
        }
    }

    public async Task DeleteStandardJobRoleAsync(int id)
    {
        try
        {
            await _dimStandardJobRoleRepository.DeleteStandardJobRoleAsync(id);
            _logger.LogInformation("Deleted DimStandardJobRole {JobRoleId}", id);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot delete, standard job role {JobRoleId} not found", id);
            throw;
        }
    }
    
    public async Task<IEnumerable<DimStandardJobRole>> GetStandardJobRolesByIndustryAsync(int industryFieldId)
    {
        _logger.LogInformation("Fetching standard job roles for IndustryFieldId: {IndustryFieldId}", industryFieldId);
        if (industryFieldId <= 0)
        {
            _logger.LogWarning("Invalid IndustryFieldId provided for GetStandardJobRolesByIndustryAsync: {IndustryFieldId}", industryFieldId);
            throw new ArgumentException("IndustryFieldId must be a positive integer.");
        }
        var roles = await _dimStandardJobRoleRepository.GetStandardJobRolesByIndustryAsync(industryFieldId);
        _logger.LogInformation("Fetched {Count} standard job roles for IndustryFieldId: {IndustryFieldId}", roles.Count(), industryFieldId);
        return roles;
    }
}