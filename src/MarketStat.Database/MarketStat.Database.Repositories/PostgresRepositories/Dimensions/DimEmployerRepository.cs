using MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;
using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Context;
using MarketStat.Database.Core.Repositories.Dimensions;
using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketStat.Database.Repositories.PostgresRepositories.Dimensions;

public class DimEmployerRepository : IDimEmployerRepository
{
    private readonly MarketStatDbContext _dbContext;

    public DimEmployerRepository(MarketStatDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task AddEmployerAsync(DimEmployer employer)
    {
        var dbEmployer = DimEmployerConverter.ToDbModel(employer);
        await _dbContext.DimEmployers.AddAsync(dbEmployer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<DimEmployer> GetEmployerByIdAsync(int employerId)
    {
        var dbEmployer = await _dbContext.DimEmployers.FindAsync(employerId)
            ?? throw new KeyNotFoundException($"Employer {employerId} not found.");
        return DimEmployerConverter.ToDomain(dbEmployer);
    }

    public async Task<IEnumerable<DimEmployer>> GetAllEmployersAsync()
    {
        var allDbEmployers = await _dbContext.DimEmployers.ToListAsync();
        return allDbEmployers.Select(DimEmployerConverter.ToDomain);
    }

    public async Task UpdateEmployerAsync(DimEmployer employer)
    {
        var dbEmployer = await _dbContext.DimEmployers.FindAsync(employer.EmployerId)
            ?? throw new KeyNotFoundException($"Cannot update {employer.EmployerId}.");
        dbEmployer.EmployerName = employer.EmployerName;
        dbEmployer.Industry = employer.Industry;
        dbEmployer.IsPublic = employer.IsPublic;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteEmployerAsync(int employerId)
    {
        var dbModel = await _dbContext.DimEmployers.FindAsync(employerId)
            ?? throw new KeyNotFoundException($"Cannot delete {employerId}.");
        _dbContext.DimEmployers.Remove(dbModel);
        await _dbContext.SaveChangesAsync();
    }
}