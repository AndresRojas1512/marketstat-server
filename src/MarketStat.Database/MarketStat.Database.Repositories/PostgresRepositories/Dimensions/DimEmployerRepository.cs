using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Common.Exceptions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployerRepository : BaseRepository, IDimEmployerRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimEmployerRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddEmployerAsync(DimEmployer employer)
    {
        var dbModel = new DimEmployerDbModel(
            employerId: 0,
            employerName: employer.EmployerName,
            isPublic: employer.IsPublic
        );
        await _dbContext.DimEmployers.AddAsync(dbModel);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An employer named '{employer.EmployerName}' already exists.");
        }
        employer.EmployerId = dbModel.EmployerId;
    }

    public async Task<DimEmployer> GetEmployerByIdAsync(int employerId)
    {
        var dbEmployer = await _dbContext.DimEmployers.FindAsync(employerId);
        if (dbEmployer is null)
            throw new NotFoundException($"Employer with ID {employerId} not found.");
        return DimEmployerConverter.ToDomain(dbEmployer);
    }

    public async Task<IEnumerable<DimEmployer>> GetAllEmployersAsync()
    {
        var allDbEmployers = await _dbContext.DimEmployers.ToListAsync();
        return allDbEmployers.Select(DimEmployerConverter.ToDomain);
    }

    public async Task UpdateEmployerAsync(DimEmployer employer)
    {
        var dbEmployer = await _dbContext.DimEmployers.FindAsync(employer.EmployerId);
        if (dbEmployer is null)
            throw new NotFoundException($"Employer with ID {employer.EmployerId} not found.");
        
        dbEmployer.EmployerName = employer.EmployerName;
        dbEmployer.IsPublic = employer.IsPublic;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pg
                  && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"An employer named '{employer.EmployerName}' already exists.");
        }
    }

    public async Task DeleteEmployerAsync(int employerId)
    {
        var dbEmployer = await _dbContext.DimEmployers.FindAsync(employerId);
        if (dbEmployer is null)
            throw new NotFoundException($"Employer with ID {employerId} not found.");
        _dbContext.DimEmployers.Remove(dbEmployer);
        await _dbContext.SaveChangesAsync();
    }
}