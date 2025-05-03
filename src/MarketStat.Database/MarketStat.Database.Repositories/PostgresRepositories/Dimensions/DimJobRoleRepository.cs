using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimJobRoleRepository : BaseRepository, IDimJobRoleRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimJobRoleRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddJobRoleAsync(DimJobRole jobRole)
    {
        var dbJobRole = DimJobRoleConverter.ToDbModel(jobRole);
        await _dbContext.DimJobRoles.AddAsync(dbJobRole);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<DimJobRole> GetJobRoleByIdAsync(int jobRoleId)
    {
        var dbJobRole = await _dbContext.DimJobRoles.FindAsync(jobRoleId) 
                        ?? throw new KeyNotFoundException($"Job Role {jobRoleId} not found.");
        return DimJobRoleConverter.ToDomain(dbJobRole);
    }

    public async Task<IEnumerable<DimJobRole>> GetAllJobRolesAsync()
    {
        var allDbJobRoles = await _dbContext.DimJobRoles.ToListAsync();
        return allDbJobRoles.Select(DimJobRoleConverter.ToDomain);
    }

    public async Task UpdateJobRoleAsync(DimJobRole jobRole)
    {
        var dbJobRole = await _dbContext.DimJobRoles.FindAsync(jobRole.JobRoleId) 
                        ?? throw new KeyNotFoundException($"Cannot update Job Role {jobRole.JobRoleId}.");
        dbJobRole.JobRoleTitle = jobRole.JobRoleTitle;
        dbJobRole.StandardJobRoleId = jobRole.StandardJobRoleId;
        dbJobRole.HierarchyLevelId = jobRole.HierarchyLevelId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteJobRoleAsync(int jobRoleId)
    {
        var dbJobRole = await _dbContext.DimJobRoles.FindAsync(jobRoleId) 
                        ?? throw new KeyNotFoundException($"Cannot delete Job Role {jobRoleId}.");
        _dbContext.DimJobRoles.Remove(dbJobRole);
        await _dbContext.SaveChangesAsync();
    }
}