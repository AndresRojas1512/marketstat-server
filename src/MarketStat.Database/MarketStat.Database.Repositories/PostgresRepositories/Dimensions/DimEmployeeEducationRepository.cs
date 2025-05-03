using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployeeEducationRepository : BaseRepository, IDimEmployeeEducationRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimEmployeeEducationRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddEmployeeEducationAsync(DimEmployeeEducation link)
    {
        var dbLink = DimEmployeeEducationConverter.ToDbModel(link);
        await _dbContext.DimEmployeeEducations.AddAsync(dbLink);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<DimEmployeeEducation> GetEmployeeEducationAsync(int employeeId, int educationId)
    {
        var dbLink = await _dbContext.DimEmployeeEducations.FindAsync(employeeId, educationId) 
                     ?? throw new KeyNotFoundException($"EmployeeEducation ({employeeId}, {educationId}) not found.");
        return DimEmployeeEducationConverter.ToDomain(dbLink);
    }

    public async Task<IEnumerable<DimEmployeeEducation>> GetEducationsByEmployeeIdAsync(int employeeId)
    {
        var dbLinks = await _dbContext.DimEmployeeEducations
            .Where(e => e.EmployeeId == employeeId)
            .ToListAsync();
        return dbLinks.Select(DimEmployeeEducationConverter.ToDomain);
    }

    public async Task<IEnumerable<DimEmployeeEducation>> GetAllEmployeeEducationsAsync()
    {
        var dbLinks = await _dbContext.DimEmployeeEducations.ToListAsync();
        return dbLinks.Select(DimEmployeeEducationConverter.ToDomain);
    }

    public async Task UpdateEmployeeEducationAsync(DimEmployeeEducation link)
    {
        var dbLink = await _dbContext.DimEmployeeEducations.FindAsync(link.EmployeeId, link.EducationId) 
                     ?? throw new KeyNotFoundException($"Cannot update EmployeeEducation ({link.EmployeeId}, {link.EducationId}) not found.");
        dbLink.GraduationYear = link.GraduationYear;
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task DeleteEmployeeEducationAsync(int employeeId, int educationId)
    {
        var dbLink = await _dbContext.DimEmployeeEducations.FindAsync(employeeId, educationId)
                     ?? throw new KeyNotFoundException(
                         $"Cannot delete: EmployeeEducation ({employeeId}, {educationId}) not found.");
        _dbContext.DimEmployeeEducations.Remove(dbLink);
        await _dbContext.SaveChangesAsync();
    }
}