using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEducationLevelRepository : BaseRepository, IDimEducationLevelRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimEducationLevelRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddEducationLevelAsync(DimEducationLevel educationLevel)
    {
        var dbEducationLevel = DimEducationLevelConverter.ToDbModel(educationLevel);
        await _dbContext.DimEducationLevels.AddAsync(dbEducationLevel);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<DimEducationLevel> GetEducationLevelByIdAsync(int id)
    {
        var dbEducationLevel = await _dbContext.DimEducationLevels.FindAsync(id)
                               ?? throw new KeyNotFoundException($"EducationLevel {id} not found.");
        return DimEducationLevelConverter.ToDomain(dbEducationLevel);
    }

    public async Task<IEnumerable<DimEducationLevel>> GetAllEducationLevelsAsync()
    {
        var allEducationLevels = await _dbContext.DimEducationLevels.ToListAsync();
        return allEducationLevels.Select(DimEducationLevelConverter.ToDomain);
    }

    public async Task UpdateEducationLevelsAsync(DimEducationLevel educationLevel)
    {
        var dbEducationLevel = await _dbContext.DimEducationLevels.FindAsync(educationLevel.EducationLevelId)
                               ?? throw new KeyNotFoundException(
                                   $"Cannot update: EducationLevel {educationLevel.EducationLevelId} not found.");
        dbEducationLevel.EducationLevelName = educationLevel.EducationLevelName;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteEducationLevelAsync(int id)
    {
        var dbEducationLevel = await _dbContext.DimEducationLevels.FindAsync(id)
                               ?? throw new KeyNotFoundException($"Cannot delete: EducationLevel {id} not found.");
        _dbContext.DimEducationLevels.Remove(dbEducationLevel);
        await _dbContext.SaveChangesAsync();
    }
}