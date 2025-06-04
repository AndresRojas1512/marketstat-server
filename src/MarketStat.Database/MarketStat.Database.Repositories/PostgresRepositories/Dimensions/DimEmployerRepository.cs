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
        var dbModel = new DimEmployerDbModel
        {
            EmployerName = employer.EmployerName,
            Inn = employer.Inn,
            Ogrn = employer.Ogrn,
            Kpp = employer.Kpp,
            RegistrationDate = employer.RegistrationDate,
            LegalAddress = employer.LegalAddress,
            Website = employer.Website,
            ContactEmail = employer.ContactEmail,
            ContactPhone = employer.ContactPhone
        };
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
        var dbEmployer = await _dbContext.DimEmployers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployerId == employerId);
        if (dbEmployer == null)
            throw new NotFoundException($"Employer with ID {employerId} not found.");
        return DimEmployerConverter.ToDomain(dbEmployer);
    }

    public async Task<IEnumerable<DimEmployer>> GetAllEmployersAsync()
    {
        var allDbEmployers = await _dbContext.DimEmployers
            .AsNoTracking()
            .OrderBy(e => e.EmployerName)
            .ToListAsync();
        return allDbEmployers.Select(DimEmployerConverter.ToDomain);
    }

    public async Task UpdateEmployerAsync(DimEmployer employer)
    {
        var dbEmployer = await _dbContext.DimEmployers
            .FirstOrDefaultAsync(e => e.EmployerId == employer.EmployerId);
        
        if (dbEmployer == null)
            throw new NotFoundException($"Employer with ID {employer.EmployerId} not found.");
        
        dbEmployer.EmployerName = employer.EmployerName;
        dbEmployer.Inn = employer.Inn;
        dbEmployer.Ogrn = employer.Ogrn;
        dbEmployer.Kpp = employer.Kpp;
        dbEmployer.RegistrationDate = employer.RegistrationDate;
        dbEmployer.LegalAddress = employer.LegalAddress;
        dbEmployer.Website = employer.Website;
        dbEmployer.ContactEmail = employer.ContactEmail;
        dbEmployer.ContactPhone = employer.ContactPhone;
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
            when (dbEx.InnerException is PostgresException pgEx &&
                  pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"Updating employer resulted in a conflict (name, INN, or OGRN already exists for another record)");
        }
    }

    public async Task DeleteEmployerAsync(int employerId)
    {
        var dbEmployer = await _dbContext.DimEmployers.FindAsync(employerId);
        if (dbEmployer == null)
            throw new NotFoundException($"Employer with ID {employerId} not found.");
        _dbContext.DimEmployers.Remove(dbEmployer);
        await _dbContext.SaveChangesAsync();
    }
}