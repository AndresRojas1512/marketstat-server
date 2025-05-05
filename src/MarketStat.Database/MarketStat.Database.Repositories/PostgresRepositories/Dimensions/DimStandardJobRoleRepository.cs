using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimStandardJobRoleRepository : BaseRepository, IDimStandardJobRoleRepository
{
    private readonly MarketStatDbContext _context;

    public DimStandardJobRoleRepository(MarketStatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task AddStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        var dbModel = new DimStandardJobRoleDbModel(
            standardJobRoleId: 0,
            standardJobRoleTitle: jobRole.StandardJobRoleTitle,
            industryFieldId: jobRole.IndustryFieldId
        );
        await _context.DimStandardJobRoles.AddAsync(dbModel);
        await _context.SaveChangesAsync();
        jobRole.StandardJobRoleId = dbModel.StandardJobRoleId;
    }

    public async Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(int id)
    {
        var dbJobRole = await _context.DimStandardJobRoles.FindAsync(id) 
                        ?? throw new KeyNotFoundException($"Standard job role {id} not found");
        return DimStandardJobRoleConverter.ToDomain(dbJobRole);
    }

    public async Task<IEnumerable<DimStandardJobRole>> GetAllStandardJobRolesAsync()
    {
        var allJobRoles = await _context.DimStandardJobRoles.ToListAsync();
        return allJobRoles.Select(DimStandardJobRoleConverter.ToDomain);
    }

    public async Task UpdateStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        var dbJobRole = await _context.DimStandardJobRoles.FindAsync(jobRole.StandardJobRoleId)
                        ?? throw new KeyNotFoundException(
                            $"Cannot update: Standard job role {jobRole.StandardJobRoleId} not found");
        dbJobRole.StandardJobRoleTitle = jobRole.StandardJobRoleTitle;
        dbJobRole.IndustryFieldId = jobRole.IndustryFieldId;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStandardJobRoleAsync(int id)
    {
        var dbJobRole = await _context.DimStandardJobRoles.FindAsync(id) 
                        ?? throw new KeyNotFoundException($"Cannot delete: Standard job role {id} not found");
        _context.DimStandardJobRoles.Remove(dbJobRole);
        await _context.SaveChangesAsync();
    }
}