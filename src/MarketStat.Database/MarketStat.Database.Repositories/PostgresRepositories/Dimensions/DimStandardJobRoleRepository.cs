using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimStandardJobRoleRepository : IDimStandardJobRoleRepository
{
    private readonly MarketStatDbContext _context;

    public DimStandardJobRoleRepository(MarketStatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task AddStandardJobRoleAsync(DimStandardJobRole jobRole)
    {
        var dbJobRole = DimStandardJobRoleConverter.ToDbModel(jobRole);
        await _context.DimStandardJobRoles.AddAsync(dbJobRole);
        await _context.SaveChangesAsync();
    }

    public async Task<DimStandardJobRole> GetStandardJobRoleByIdAsync(long id)
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