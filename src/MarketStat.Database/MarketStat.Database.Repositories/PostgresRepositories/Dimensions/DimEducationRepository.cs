using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEducationRepository : BaseRepository, IDimEducationRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimEducationRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddEducationAsync(DimEducation education)
    {
        var dbModel = DimEducationConverter.ToDbModel(education);
        await _dbContext.DimEducations.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An education with code '{education.SpecialtyCode}' already exists.");
        }
        
        education.EducationId = dbModel.EducationId;
    }

    public async Task<DimEducation> GetEducationByIdAsync(int educationId)
    {
        var dbEducation = await _dbContext.DimEducations.FindAsync(educationId);
        if (dbEducation is null)
            throw new NotFoundException($"Education with ID {educationId} not found.");
        return DimEducationConverter.ToDomain(dbEducation);
    }

    public async Task<IEnumerable<DimEducation>> GetAllEducationsAsync()
    {
        var allDbEducations = await _dbContext.DimEducations.ToListAsync();
        return allDbEducations.Select(DimEducationConverter.ToDomain);
    }

    public async Task UpdateEducationAsync(DimEducation education)
    {
        var dbEducation = await _dbContext.DimEducations.FindAsync(education.EducationId);
        if (dbEducation is null)
            throw new NotFoundException($"Education with ID {education.EducationId} not found.");
        
        dbEducation.SpecialtyName = education.SpecialtyName;
        dbEducation.SpecialtyCode = education.SpecialtyCode;
        dbEducation.EducationLevelName = education.EducationLevelName;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An education with code '{education.SpecialtyCode}' already exists.");
        }
    }

    public async Task DeleteEducationAsync(int educationId)
    {
        var dbEducation = await _dbContext.DimEducations.FindAsync(educationId);
        if (dbEducation is null)
            throw new NotFoundException($"Education with ID {educationId} not found.");
        _dbContext.DimEducations.Remove(dbEducation);
        await _dbContext.SaveChangesAsync();
    }
}