using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"Employee {link.EmployeeId} is already linked with eduction {link.EducationId}.");
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException(
                $"Either employee {link.EmployeeId} or education {link.EducationId} does not exist.");
        }
    }

    public async Task<DimEmployeeEducation> GetEmployeeEducationAsync(int employeeId, int educationId)
    {
        var dbLink = await _dbContext.DimEmployeeEducations.FindAsync(employeeId, educationId);
        if (dbLink is null)
            throw new NotFoundException($"Link ({employeeId}, {educationId}) not found.");
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
        var dbLink = await _dbContext.DimEmployeeEducations.FindAsync(link.EmployeeId, link.EducationId);
        if (dbLink is null)
            throw new NotFoundException($"Link ({link.EmployeeId}, {link.EducationId}) not found.");
        
        dbLink.GraduationYear = link.GraduationYear;
        
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task DeleteEmployeeEducationAsync(int employeeId, int educationId)
    {
        var dbLink = await _dbContext.DimEmployeeEducations.FindAsync(employeeId, educationId);
        if (dbLink is null)
            throw new NotFoundException($"Link ({employeeId}, {educationId}) not found.");
        _dbContext.DimEmployeeEducations.Remove(dbLink);
        await _dbContext.SaveChangesAsync();
    }
}