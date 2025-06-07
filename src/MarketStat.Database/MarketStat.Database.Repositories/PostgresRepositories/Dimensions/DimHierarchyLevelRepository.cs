using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        var dbModel = DimHierarchyLevelConverter.ToDbModel(dimHierarchyLevel);
            
        await _dbContext.DimHierarchyLevels.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx 
                  && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"A hierarchy level with the name '{dimHierarchyLevel.HierarchyLevelName}' or code '{dimHierarchyLevel.HierarchyLevelCode}' already exists.");
        }
        dimHierarchyLevel.HierarchyLevelId = dbModel.HierarchyLevelId;
    }

    public async Task<IEnumerable<DimHierarchyLevel>> GetAllHierarchyLevelsAsync()
    {
        var allDbHierarchyLevels = await _dbContext.DimHierarchyLevels
            .AsNoTracking()
            .OrderBy(h => h.HierarchyLevelId)
            .ToListAsync();
        return allDbHierarchyLevels.Select(DimHierarchyLevelConverter.ToDomain);
    }

    public async Task<DimHierarchyLevel> GetHierarchyLevelByIdAsync(int id)
    {
        var dbHierarchyLevel = await _dbContext.DimHierarchyLevels
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.HierarchyLevelId == id);

        if (dbHierarchyLevel == null)
        {
            throw new NotFoundException($"Hierarchy level with ID {id} not found.");
        }
        return DimHierarchyLevelConverter.ToDomain(dbHierarchyLevel);
    }

    public async Task UpdateHierarchyLevelAsync(DimHierarchyLevel dimHierarchyLevel)
    {
        var dbHierarchyLevel = await _dbContext.DimHierarchyLevels
            .FirstOrDefaultAsync(h => h.HierarchyLevelId == dimHierarchyLevel.HierarchyLevelId);

        if (dbHierarchyLevel == null)
        {
            throw new NotFoundException($"Hierarchy level with ID {dimHierarchyLevel.HierarchyLevelId} not found.");
        }
            
        dbHierarchyLevel.HierarchyLevelName = dimHierarchyLevel.HierarchyLevelName;
        dbHierarchyLevel.HierarchyLevelCode = dimHierarchyLevel.HierarchyLevelCode;
            
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx 
                  && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"Updating hierarchy level resulted in a conflict. The name '{dimHierarchyLevel.HierarchyLevelName}' or code '{dimHierarchyLevel.HierarchyLevelCode}' may already exist.");
        }
    }

    public async Task DeleteHierarchyLevelAsync(int id)
    {
        var dbHierarchyLevel = await _dbContext.DimHierarchyLevels.FindAsync(id);
        if (dbHierarchyLevel == null)
        {
            throw new NotFoundException($"Hierarchy level with ID {id} not found.");
        }
        _dbContext.DimHierarchyLevels.Remove(dbHierarchyLevel);
        await _dbContext.SaveChangesAsync();
    }
}