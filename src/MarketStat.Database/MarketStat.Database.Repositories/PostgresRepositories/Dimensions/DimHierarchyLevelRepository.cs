using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimHierarchyLevelRepository : BaseRepository, IDimHierarchyLevelRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimHierarchyLevelRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddHierarchyLevelAsync(DimHierarchyLevel dimHierarchyLevel)
    {
        var dbModel = new DimHierarchyLevelDbModel(
            hierarchyLevelId: 0,
            hierarchyLevelName: dimHierarchyLevel.HierarchyLevelName
        );
        await _dbContext.DimHierarchyLevels.AddAsync(dbModel);
        await _dbContext.SaveChangesAsync();
        dimHierarchyLevel.HierarchyLevelId = dbModel.HierarchyLevelId;
    }

    public async Task<IEnumerable<DimHierarchyLevel>> GetAllHierarchyLevelsAsync()
    {
        var allDbHierarchyLevels = await _dbContext.DimHierarchyLevels.ToListAsync();
        return allDbHierarchyLevels.Select(DimHierarchyLevelConverter.ToDomain);
    }

    public async Task<DimHierarchyLevel> GetHierarchyLevelByIdAsync(int id)
    {
        var dbHierarchyLevel = await _dbContext.DimHierarchyLevels.FindAsync(id) 
                               ?? throw new KeyNotFoundException($"HierarchyLevel {id} not found.");
        return DimHierarchyLevelConverter.ToDomain(dbHierarchyLevel);
    }

    public async Task UpdateHierarchyLevelAsync(DimHierarchyLevel dimHierarchyLevel)
    {
        var dbHierarchyLevel = await _dbContext.DimHierarchyLevels.FindAsync(dimHierarchyLevel.HierarchyLevelId)
                               ?? throw new KeyNotFoundException(
                                   $"Cannot update: HierarchyLevel {dimHierarchyLevel.HierarchyLevelId} not found.");
        dbHierarchyLevel.HierarchyLevelName = dimHierarchyLevel.HierarchyLevelName;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteHierarchyLevelAsync(int id)
    {
        var dbHierarchyLevel = await _dbContext.DimHierarchyLevels.FindAsync(id)
                               ?? throw new KeyNotFoundException($"Cannot update: HierarchyLevel {id} not found.");
        _dbContext.DimHierarchyLevels.Remove(dbHierarchyLevel);
        await _dbContext.SaveChangesAsync();
    }
}