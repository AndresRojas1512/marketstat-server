using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEducationRepository : IDimEducationRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimEducationRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddEducationAsync(DimEducation education)
    {
        var dbEducation = DimEducationConverter.ToDbModel(education);
        await _dbContext.DimEducations.AddAsync(dbEducation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<DimEducation> GetEducationByIdAsync(int educationId)
    {
        var dbEducation = await _dbContext.DimEducations.FindAsync(educationId) 
                          ?? throw new KeyNotFoundException($"Education {educationId} not found.");
        return DimEducationConverter.ToDomain(dbEducation);
    }

    public async Task<IEnumerable<DimEducation>> GetAllEducationsAsync()
    {
        var allDbEducations = await _dbContext.DimEducations.ToListAsync();
        return allDbEducations.Select(DimEducationConverter.ToDomain);
    }

    public async Task UpdateEducationAsync(DimEducation education)
    {
        var dbEducation = await _dbContext.DimEducations.FindAsync(education.EducationId) 
                          ?? throw new KeyNotFoundException($"Cannot update Education {education.EducationId}.");
        dbEducation.EducationId = education.EducationId;
        dbEducation.Specialization = education.Specialization;
        dbEducation.EducationLevelId = education.EducationLevelId;
        dbEducation.IndustryFieldId = education.IndustryFieldId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteEducationAsync(int educationId)
    {
        var dbEducation = await _dbContext.DimEducations.FindAsync(educationId) 
                          ?? throw new KeyNotFoundException($"Cannot delete Education {educationId}.");
        _dbContext.DimEducations.Remove(dbEducation);
        await _dbContext.SaveChangesAsync();
    }
}