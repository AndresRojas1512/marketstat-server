using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
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
    
    public async Task<DimStandardJobRole> CreateStandardJobRoleAsync(string jobRoleTitle, int industryFieldId)
    {
        DimStandardJobRoleValidator.ValidateForCreate(jobRoleTitle, industryFieldId);
        var all = await _dimStandardJobRoleRepository.GetAllStandardJobRolesAsync();
        if (all.Any(r => 
                r.StandardJobRoleTitle.Equals(jobRoleTitle, StringComparison.OrdinalIgnoreCase) &&
                r.IndustryFieldId == industryFieldId
            ))
            throw new Exception($"Could not create job role '{jobRoleTitle}'");

        var role = new DimStandardJobRole(0, jobRoleTitle, industryFieldId);
        try
        {
            await _dimStandardJobRoleRepository.AddStandardJobRoleAsync(role);
            _logger.LogInformation("Created DimStandardJobRole {JobRoleId}", role.StandardJobRoleId);
            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DimStandardJobRole (duplicate {JobRoleId})", role.StandardJobRoleId);
            throw new Exception($"Could not create job role '{jobRoleTitle}'");
        }
    }

    public async Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(int id)
    {
        try
        {
            return await _dimStandardJobRoleRepository.GetStandardJobRoleByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "StandardJobRole {JobRoleId} not found", id);
            throw new Exception($"Standard job role {id} was not found.");
        }
    }

    public async Task<IEnumerable<DimStandardJobRole>> GetAllStandardJobRolesAsync()
    {
        var list = await _dimStandardJobRoleRepository.GetAllStandardJobRolesAsync();
        _logger.LogInformation("Fetched {Count} JobRole records", list.Count());
        return list;
    }

    public async Task<DimStandardJobRole> UpdateStandardJobRoleAsync(int id, string jobRoleTitle, int industryFieldId)
    {
        DimStandardJobRoleValidator.ValidateForUpdate(id, jobRoleTitle, industryFieldId);
        try
        {
            var existing = await _dimStandardJobRoleRepository.GetStandardJobRoleByIdAsync(id);
            existing.StandardJobRoleTitle   = jobRoleTitle;
            existing.IndustryFieldId = industryFieldId;
            await _dimStandardJobRoleRepository.UpdateStandardJobRoleAsync(existing);
            _logger.LogInformation("Updated DimStandardJobRole {JobRoleId}", id);
            return existing;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot update, StandardJobRole {JobRoleId} not found", id);
            throw new Exception($"Cannot update: job role {id} not found.");
        }
    }

    public async Task DeleteStandardJobRoleAsync(int id)
    {
        try
        {
            await _dimStandardJobRoleRepository.DeleteStandardJobRoleAsync(id);
            _logger.LogInformation("Deleted DimStandardJobRole {JobRoleId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot delete, DimStandardJobRole {JobRoleId} not found", id);
            throw new Exception($"Cannot delete: standard job role {id} not found.");
        }
    }
}