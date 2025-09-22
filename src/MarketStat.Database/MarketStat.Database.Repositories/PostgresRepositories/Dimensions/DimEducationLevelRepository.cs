using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        var dbModel = DimEducationLevelConverter.ToDbModel(educationLevel);
        await _dbContext.DimEducationLevels.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An education level '{dbModel.EducationLevelName}' already exists.");
        }
        educationLevel.EducationLevelId = dbModel.EducationLevelId;
    }

    public async Task<DimEducationLevel> GetEducationLevelByIdAsync(int id)
    {
        var dbEducationLevel = await _dbContext.DimEducationLevels.FindAsync(id);
        if (dbEducationLevel is null)
            throw new NotFoundException($"Education level with ID {id} not found.");
        return DimEducationLevelConverter.ToDomain(dbEducationLevel);
    }

    public async Task<IEnumerable<DimEducationLevel>> GetAllEducationLevelsAsync()
    {
        var allEducationLevels = await _dbContext.DimEducationLevels.ToListAsync();
        return allEducationLevels.Select(DimEducationLevelConverter.ToDomain);
    }

    public async Task UpdateEducationLevelAsync(DimEducationLevel educationLevel)
    {
        var dbEducationLevel = await _dbContext.DimEducationLevels.FindAsync(educationLevel.EducationLevelId);
        if (dbEducationLevel is null)
            throw new NotFoundException($"Education level with ID {educationLevel.EducationLevelId} not found.");
        
        dbEducationLevel.EducationLevelName = educationLevel.EducationLevelName;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An education level '{dbEducationLevel.EducationLevelName}' already exists.");
        }
    }

    public async Task DeleteEducationLevelAsync(int id)
    {
        var dbEducationLevel = await _dbContext.DimEducationLevels.FindAsync(id);
        if (dbEducationLevel is null)
            throw new NotFoundException($"Education level with ID {id} not found.");
        _dbContext.DimEducationLevels.Remove(dbEducationLevel);
        await _dbContext.SaveChangesAsync();
    }
}